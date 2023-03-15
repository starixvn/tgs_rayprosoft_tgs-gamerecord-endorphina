using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;

internal class GamePullRecordCommandService : ICommandService<GamePullRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<GamePullRecordCommandService> _logger;
    private readonly IGameInterfaceEndorphinaClient _endorphinaClient;
    private readonly IGameRecordEndorphinaSessionClient _sessionClient;
    private readonly IGameRoundsWorkClient _client;
    private readonly IUtensil _utensil;

    public GamePullRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        ILogger<GamePullRecordCommandService> logger,
        IGameInterfaceEndorphinaClient endorphinaClient,
        IGameRecordEndorphinaSessionClient sessionClient,
        IGameRoundsWorkClient client,
        IUtensil utensil)
    {
        _gameRecordInfo = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _endorphinaClient = endorphinaClient ?? throw new ArgumentNullException(nameof(endorphinaClient));
        _sessionClient = sessionClient ?? throw new ArgumentNullException(nameof(sessionClient));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(GamePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var record = command.Session;
        try
        {
            var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

            var sessionInfo = await _endorphinaClient
                .GameSessionInfoAsync(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    record.Token,
                    cancellationToken)
                .ConfigureAwait(false);

            if (sessionInfo == null)
            {
                _logger.LogInformation("No Session Info (Session Id:{token})", record.Token);
                return;
            }

            if (sessionInfo.ErrorsPending != 0)
            {
                _logger.LogInformation("Please contact endorphina support for resolution (Session Id:{token})", record.Token);
                return;
            }

            var gameExternalId = sessionInfo.GameName;
            var externalRoundId = sessionInfo.Id;

            var IsClosed = sessionInfo.Closed.ToDateTimeOffset() != DateTimeOffset.UnixEpoch;

            var sessionStatistics = await _endorphinaClient
                .GameSessionStatisticsAsync(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    record.Token,
                    cancellationToken)
                .ConfigureAwait(false);

            bool success = true;
            var playerModel = await _utensil
                .GetPlayerAsync(
                    playerCode: record.PlayerCode,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var gameId = await _utensil
                .GetGameIdAsync(
                    gameProviderId: gameProviderId,
                    gameExternalId: gameExternalId,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

            if (playerModel == null || !gameId.HasValue)
            {
                success = false;
            }

            Guid? gameProviderConfigurationId = default;
            if (playerModel != null)
            {
                gameProviderConfigurationId = await _utensil
                    .GetBrandGameProviderConfigurationIdAsync(
                        brandId: playerModel.BrandId,
                        gameProviderId: gameProviderId,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!gameProviderConfigurationId.HasValue)
                {
                    success = false;
                }
            }

            var status = IsClosed ? RoundBiddingStatus.Closed : RoundBiddingStatus.Open;

            var playerWinLoss = 0.0m;
            DateTimeOffset? externalClosedOn = null;
            var betAmount = 0.0m;
            var validBetAmount = 0.0m;
            var settleAmount = 0.0m;
            var turnover = 0.0m;
            if (status == RoundBiddingStatus.Closed && sessionStatistics != null)
            {
                betAmount = sessionStatistics.Bet.ToDecimal().GetValueOrDefault(0);
                validBetAmount = sessionStatistics.Bet.ToDecimal().GetValueOrDefault(0);
                externalClosedOn = sessionInfo.Closed.ToDateTimeOffset();
                settleAmount = sessionStatistics.Win.ToDecimal().GetValueOrDefault(0);
                turnover = sessionStatistics.Bet.ToDecimal().GetValueOrDefault(0);
                playerWinLoss = sessionStatistics.Win.ToDecimal().GetValueOrDefault(0) - sessionStatistics.Bet.ToDecimal().GetValueOrDefault(0);
            }

            var clientStatus = IsClosed
                ? playerWinLoss > 0
                    ? RoundBiddingClientStatus.Win
                    : playerWinLoss == 0
                        ? RoundBiddingClientStatus.Draw
                        : RoundBiddingClientStatus.Loss
                : RoundBiddingClientStatus.Open;

            var externalCreatedOn = sessionInfo.Created.ToDateTimeOffset();

            var additionalData = new
            {
                ExternalRoundId = sessionInfo.Id,
                sessionInfo.Username,
                sessionInfo.GameName,
                sessionInfo.Currency,
                sessionStatistics?.Games,
                BetAmount = sessionStatistics?.Bet.ToDecimal(),
                WinAmount = sessionStatistics?.Win.ToDecimal(),
                sessionStatistics?.Jackpots,
                JackpotWinAmount = sessionStatistics?.JackpotWin.ToDecimal(),
            };

            if (success)
            {
                await _client
                .RoundBidding(
                    gameProviderId: gameProviderId,
                    gameProviderCode: _gameRecordInfo.GameProviderCode,
                    externalRoundId: externalRoundId,
                    gameId: gameId!.Value,
                    externalGameId: gameExternalId,
                    playerCode: playerModel!.PlayerCode,
                    walletId: playerModel!.DefaultWalletId,
                    currencyCode: playerModel!.DefaultWalletCurrencyCode,
                    brandId: playerModel!.BrandId,
                    gameProviderConfigurationId: gameProviderConfigurationId!.Value,
                    status: status,
                    clientStatus: clientStatus,
                    externalCreatedOn: externalCreatedOn,
                    externalClosedOn: externalClosedOn,
                    bet: betAmount,
                    validBet: validBetAmount,
                    settle: settleAmount,
                    commission: 0,
                    turnover: turnover,
                    playerWinLoss: playerWinLoss,
                    additionalData: string.Empty,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            }
            else
            {
                await _client
                .RoundMissing(
                    gameProviderId: gameProviderId,
                    gameProviderCode: _gameRecordInfo.GameProviderCode,
                    externalRoundId: externalRoundId,
                    gameId: gameId ?? -1,
                    externalGameId: gameExternalId,
                    playerCode: playerModel?.PlayerCode ?? string.Empty,
                    data: string.Empty,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            await _sessionClient.UpdateSessionRetryAsync(record.Token, cancellationToken).ConfigureAwait(false);
            _logger.LogError(ex, "遊戲紀錄處理發生例外.({record})", record);
        }
    }
}
