using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Tgs.GameRecord.Handle.Client;

namespace Tgs.GameRecord.Endorphina.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Normal_Process()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "ENDOR",
			GameProviderConfiguration = "ENDORConfiguration",
			HangfirePrefix = "gamerecord-endor-hangfire:",
			HangfireLocking = "gamerecord-endor-locking:"
		});
		var fakePolicySettingsOptions = Options.Create(new PolicySettingsOptions
		{
			RetryCount = 1,
			RetrySeconds = 1,
		});
		var fakeGameRecordHandleClient = Substitute.For<IGameRecordHandleClient>();

		var externalRoundId = "AA8BB60F5A134264BE838CF6EBA7C8F1";
		var externalGameId = "endorphina_MinotaurDice";
		var playerCode = "4ucmqplxb029";
		var externalCurrency = "CNY";
		var externalCreatedOn = new DateTimeOffset(2023, 02, 21, 07, 14, 46, TimeSpan.Zero);
		var externalClosedOn = new DateTimeOffset(2023, 02, 21, 07, 19, 37, TimeSpan.Zero);
		var bet = 2.0m;
		var win = 0.50m;
		var games = 4;
		var jackpots = 0;
		var jackpotWin = 0.00m;

		var command = new HandleRecordCommand(
			ExternalRoundId: externalRoundId,
			ExternalGameId: externalGameId,
			PlayerCode: playerCode,
			ExternalCurrency: externalCurrency,
			ExternalCreatedOn: externalCreatedOn,
			ExternalClosedOn: externalClosedOn,
			Bet: bet,
			Win: win,
			Games: games,
			Jackpots: jackpots,
			JackpotWin: jackpotWin);

		var sut = new HandleRecordCommandService(
			faceLogger,
			fakeGameRecordInfoOptions,
			fakePolicySettingsOptions,
			fakeGameRecordHandleClient);

		await sut.ExecuteAsync(command: command);

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

		_ = fakeGameRecordHandleClient
			.Received(1)
			.HandleAsync(
			source: Arg.Is(new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = fakeGameRecordInfoOptions.Value.GameProviderCode,
				PlayerCode = command.PlayerCode,
				ExternalRoundId = command.ExternalRoundId,
				ExternalBetId = command.ExternalRoundId,
				ExternalGameId = command.ExternalGameId,
				ExternalCreatedOn = command.ExternalCreatedOn,
				ExternalClosedOn = command.ExternalClosedOn,
				ExternalPayoffOn = command.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 2.0m,
				ValidBet = 2.0m,
				Settle = 0.50m,
				Commission = 0.0M,
				Turnover = 2.0m,
				WinLoss = -1.5m,
				Jackpot = 0.00m,
				AdditionalData = JsonSerializer.Serialize(additionalData, options: new() { WriteIndented = true })
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
