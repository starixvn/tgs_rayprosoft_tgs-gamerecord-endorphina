using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Tgs.GameRecord.Handle.Client;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class HandleRecordCommandService(
	ILogger<HandleRecordCommandService> logger,
	IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
	IOptions<PolicySettingsOptions> policySettingsOptions,
	IGameRecordHandleClient client) : ICommandService<HandleRecordCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfoOptions = gameRecordInfoOptions.Value;
	private readonly PolicySettingsOptions _policySettingsOptions = policySettingsOptions.Value;
	private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

	public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			var winLoss = 0.0m;
			var betState = Handle.Common.Protos.BetState.Running;
			var externalCreatedOn = command.ExternalCreatedOn;
			var externalClosedOn = DateTimeOffset.UnixEpoch;
			var bet = 0.0m;
			var validBet = 0.0m;
			var settle = 0.0m;
			var turnover = 0.0m;
			if (command.ExternalClosedOn != DateTimeOffset.UnixEpoch)
			{
				bet = command.Bet;
				validBet = command.Bet;
				externalClosedOn = command.ExternalClosedOn;
				settle = command.Win;
				turnover = command.Bet;
				winLoss = command.Win - command.Bet;
				betState = winLoss > 0
					? Handle.Common.Protos.BetState.Win
					: winLoss == 0
					? Handle.Common.Protos.BetState.Draw
					: Handle.Common.Protos.BetState.Loss;
			}

			var additionalData = new
			{
				command.ExternalRoundId,
				Username = command.PlayerCode,
				GameName = command.ExternalGameId,
				command.ExternalCurrency,
				command.Games,
				BetAmount = command.Bet,
				WinAmount = command.Win,
				command.Jackpots,
				JackpotWinAmount = command.JackpotWin
			};

			var source = new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = _gameRecordInfoOptions.GameProviderCode,
				PlayerCode = command.PlayerCode,
				ExternalRoundId = command.ExternalRoundId,
				ExternalBetId = command.ExternalRoundId,
				ExternalGameId = command.ExternalGameId,
				ExternalCreatedOn = externalCreatedOn,
				ExternalClosedOn = externalClosedOn,
				ExternalPayoffOn = externalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = betState,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = bet,
				ValidBet = validBet,
				Settle = settle,
				Commission = 0.0m,
				Turnover = turnover,
				WinLoss = winLoss,
				Jackpot = command.JackpotWin,
				AdditionalData = JsonSerializer.Serialize(additionalData, options: _jsonSerializerOptions) ?? "{}"
			};

			var retryCount = _policySettingsOptions.RetryCount;
			var retrySeconds = _policySettingsOptions.RetrySeconds;
			var retryPolicy = Policy
				.Handle<HttpRequestException>()
				.WaitAndRetryAsync(
					retryCount: retryCount,
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(retrySeconds, retryAttempt)),
					onRetry: (exception, retryTime, retryCount, context) => logger.LogWarning(exception, "{logWarning}",
						new
						{
							LogOn = DateTime.Now.ToString("u"),
							RetryTime = retryTime,
							RetryCount = retryCount,
							Data = new
							{
								source.GameProviderCode,
								source.PlayerCode,
								source.ExternalRoundId,
								source.ExternalBetId,
								source.ExternalGameId,
								ExternalCreatedOn = source.ExternalCreatedOn.ToString("u"),
								ExternalClosedOn = source.ExternalClosedOn.ToString("u"),
								source.BetState
							},
							exception.Message
						}));

			await retryPolicy.ExecuteAsync(() => client.HandleAsync(
				source: source,
				cancellationToken: cancellationToken).AsTask())
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), command);
		}
	}
}
