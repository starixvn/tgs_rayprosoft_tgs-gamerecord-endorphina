using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

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

    public ValueTask ExecuteAsync(GamePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var playerWinLoss = 0.0m;
            var wagerState = WagerStates.Running;
            var externalCreatedOn = command.ExternalCreatedOn;
            var externalClosedOn = DateTimeOffset.UnixEpoch;
            var betAmount = 0.0m;
            var validBetAmount = 0.0m;
            var settleAmount = 0.0m;
            var turnover = 0.0m;
            if (command.ExternalClosedOn != DateTimeOffset.UnixEpoch)
            {
                betAmount = command.Bet;
                validBetAmount = command.Bet;
                externalClosedOn = command.ExternalClosedOn;
                settleAmount = command.Win;
                turnover = command.Bet;
                playerWinLoss = command.Win - command.Bet;
                wagerState = playerWinLoss > 0
                    ? WagerStates.Win
                    : playerWinLoss == 0
                    ? WagerStates.Draw
                    : WagerStates.Lose;
            }

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

            return _client
                .MessageAsync(
                gameProviderCode: _gameRecordInfo.GameProviderCode,
                playerCode: command.PlayerCode,
                wagerState: wagerState,
                wagerType: WagerTypes.Normal,
                freeGameState: FreeGameStates.None,
                gambleState: GambleStates.None,
                cashoutState: CashoutStates.None,
                resettleState: ResettleStates.None,
                externalRoundId: command.ExternalRoundId,
                externalWagerId: command.ExternalRoundId,
                externalGameId: command.GameExternalId,
                externalCreatedOn: externalCreatedOn,
                externalClosedOn: externalClosedOn,
                externalPayoffOn: externalClosedOn,
                externalResettleOn: DateTimeOffset.UnixEpoch,
                bet: betAmount,
                validBet: validBetAmount,
                settle: settleAmount,
                commission: 0,
                turnover: turnover,
                winLoss: playerWinLoss,
                additionalData: JsonSerializer.Serialize(additionalData, new JsonSerializerOptions() { WriteIndented = true }) ?? string.Empty,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), command);
        }
        return ValueTask.CompletedTask;
    }
}
