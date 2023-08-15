using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;

internal class GamePullRecordCommandService : ICommandService<GamePullRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<GamePullRecordCommandService> _logger;
    private readonly IGameRecordHandleClient _client;

    public GamePullRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        ILogger<GamePullRecordCommandService> logger,
        IGameRecordHandleClient client)
    {
        _gameRecordInfo = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(GamePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var IsClosed = command.ExternalClosedOn != DateTimeOffset.UnixEpoch;

            var status = IsClosed ? GameRecordStatus.Closed : GameRecordStatus.Open;

            var playerWinLoss = 0.0m;
            DateTimeOffset? externalClosedOn = null;
            var betAmount = 0.0m;
            var validBetAmount = 0.0m;
            var settleAmount = 0.0m;
            var turnover = 0.0m;
            if (status == GameRecordStatus.Closed)
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
                    ? GameRecordClientStatus.Win
                    : playerWinLoss == 0
                        ? GameRecordClientStatus.Draw
                        : GameRecordClientStatus.Loss
                : GameRecordClientStatus.Open;

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
                .MessageAsync(
                gameProviderCode: _gameRecordInfo.GameProviderCode,
                externalRoundId: command.ExternalRoundId,
                externalGameId: command.GameExternalId,
                playerCode: command.PlayerCode,
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
            _logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), command);
        }
    }
}
