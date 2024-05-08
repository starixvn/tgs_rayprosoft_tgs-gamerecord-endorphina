using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class GamePullRecordCommandService(
	ILogger<GamePullRecordCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	IMessageSender messageSender) : ICommandService<GamePullRecordCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;
	private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

	public async ValueTask ExecuteAsync(GamePullRecordCommand command, CancellationToken cancellationToken = default)
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
				GameName = command.GameExternalId,
				command.ExternalCurrency,
				command.Games,
				BetAmount = command.Bet,
				WinAmount = command.Win,
				command.Jackpots,
				JackpotWinAmount = command.JackpotWin
			};

			var request = new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = _gameRecordInfo.GameProviderCode,
				PlayerCode = command.PlayerCode,
				ExternalRoundId = command.ExternalRoundId,
				ExternalBetId = command.ExternalRoundId,
				ExternalGameId = command.GameExternalId,
				ExternalCreatedOn = externalCreatedOn.ToTimestamp(),
				ExternalClosedOn = externalClosedOn.ToTimestamp(),
				ExternalPayoffOn = externalClosedOn.ToTimestamp(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
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
				Jackpot = 0.0M,
				AdditionalData = JsonSerializer.Serialize(additionalData, options: _jsonSerializerOptions) ?? "{}"
			};

			await messageSender.PublishAsync(
				subject: Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest,
				data: request,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This publication requests information.({request})",
				DateTime.Now.ToString("o"),
				Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest,
				new
				{
					request.GameProviderCode,
					request.PlayerCode,
					request.ExternalRoundId,
					request.ExternalBetId,
					request.ExternalGameId,
					ExternalCreatedOn = request.ExternalCreatedOn.ToDateTimeOffset(),
					ExternalClosedOn = request.ExternalClosedOn.ToDateTimeOffset(),
					request.BetState
				});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), command);
		}
	}
}
