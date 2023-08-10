using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;

internal class GamePullRecordCommandService : ICommandService<GamePullRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<GamePullRecordCommandService> _logger;
    private readonly IGameRoundsWorkClient _client;
    private readonly IUtensil _utensil;

    public GamePullRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        ILogger<GamePullRecordCommandService> logger,
        IGameRoundsWorkClient client,
        IUtensil utensil)
    {
        _gameRecordInfo = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(GamePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var gameProviderId = await _utensil
                .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var IsClosed = command.ExternalClosedOn != DateTimeOffset.UnixEpoch;

            var playerModel = await _utensil
                .GetPlayerAsync(
                playerCode: command.PlayerCode,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var gameId = await _utensil
                .GetGameIdAsync(
                gameProviderId: gameProviderId,
                gameExternalId: command.GameExternalId,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var gameProviderConfigurationId = await _utensil
                .GetBrandGameProviderConfigurationIdAsync(
                brandId: playerModel.BrandId,
                gameProviderId: gameProviderId,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            RoundStatus status = IsClosed ? RoundStatus.Closed : RoundStatus.Open;

            var playerWinLoss = 0.0m;
            DateTimeOffset? externalClosedOn = null;
            var betAmount = 0.0m;
            var validBetAmount = 0.0m;
            var settleAmount = 0.0m;
            var turnover = 0.0m;
            if (status == RoundStatus.Closed)
            {
                betAmount = command.Bet;
                validBetAmount = command.Bet;
                externalClosedOn = command.ExternalClosedOn;
                settleAmount = command.Win;
                turnover = command.Bet;
                playerWinLoss = command.Win - command.Bet;
            }

            var clientStatus = IsClosed
                ? playerWinLoss > 0
                    ? RoundClientStatus.Win
                    : playerWinLoss == 0
                        ? RoundClientStatus.Draw
                        : RoundClientStatus.Loss
                : RoundClientStatus.Open;

            var additionalData = new
            {
                command.ExternalRoundId,
                Username = command.PlayerCode,
                GameName = command.GameExternalId,
                command.ExternalCurrency,
                command.Games,
                BetAmount = command.Bet,
                WinAmount = command.Win,
                command.Jackpots,
                JackpotWinAmount = command.JackpotWin
            };

            await _client
                .RoundExchangeRateBiddingAsync(
                gameProviderId: gameProviderId,
                gameProviderCode: _gameRecordInfo.GameProviderCode,
                externalRoundId: command.ExternalRoundId,
                gameId: gameId,
                externalGameId: command.GameExternalId,
                playerCode: playerModel.PlayerCode,
                walletId: playerModel.DefaultWalletId,
                currencyCode: playerModel.DefaultWalletCurrencyCode,
                brandId: playerModel.BrandId,
                gameProviderConfigurationId: gameProviderConfigurationId,
                status: status,
                clientStatus: clientStatus,
                externalCreatedOn: command.ExternalCreatedOn,
                externalClosedOn: command.ExternalClosedOn,
                bet: betAmount,
                validBet: validBetAmount,
                settle: settleAmount,
                commission: 0,
                turnover: turnover,
                playerWinLoss: playerWinLoss,
                additionalData: JsonSerializer.Serialize(additionalData, new JsonSerializerOptions() { WriteIndented = true }) ?? string.Empty,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LogOn:{logOn} Message: game record exception.({gameRecord})", DateTime.Now.ToString("o"), command);
        }
    }
}
