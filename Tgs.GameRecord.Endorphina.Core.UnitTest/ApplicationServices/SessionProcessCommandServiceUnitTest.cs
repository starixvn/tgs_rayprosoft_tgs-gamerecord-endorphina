using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameInterface.EndorphinaService.Protos;
using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.UnitTest.ApplicationServices;

public class SessionProcessCommandServiceUnitTest
{
	[Fact]
	public async Task Normal_Process()
	{
		var fakeLogger = NullLogger<SessionProcessCommandService>.Instance;
		var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();
		var fakeSessionClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
		var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

		var apiUrl = new Uri("http://localhost/");
		var merchantId = "1234";
		var secretKey = "testSecretKey";
		var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

		var sessionInfo = new GameSessionInfoReply()
		{
			Id = "AA8BB60F5A134264BE838CF6EBA7C8F1",
			GameName = "endorphina_MinotaurDice",
			Username = "4ucmqplxb029",
			Currency = "CNY",
			Created = new DateTimeOffset(2023, 02, 21, 07, 14, 46, TimeSpan.Zero).ToTimestamp(),
			Closed = new DateTimeOffset(2023, 02, 21, 07, 19, 37, TimeSpan.Zero).ToTimestamp(),
			ErrorsPending = 0,
			AccountType = "CREDIT"
		};
		_ = fakeClient.GameSessionInfoAsync(
			apiUrl: Arg.Is(apiUrl),
			merchantId: Arg.Is(merchantId),
			secretKey: Arg.Is(secretKey),
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(sessionInfo);

		var sessionStatistics = new GameSessionStatisticsReply()
		{
			Bet = 2.00m,
			Win = 0.50m,
			Games = 4,
			Jackpots = 0,
			JackpotWin = 0.00m,
			Deposit = 2.00m,
			Withdraw = 0.50m,
			ErrorsPending = 0
		};
		_ = fakeClient.GameSessionStatisticsAsync(
			apiUrl: Arg.Is(apiUrl),
			merchantId: Arg.Is(merchantId),
			secretKey: Arg.Is(secretKey),
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(sessionStatistics);

		var sut = new SessionProcessCommandService(
			fakeLogger,
			fakeClient,
			fakeSessionClient,
			fakeService);

		var command = new SessionProcessCommand(
			ApiUrl: apiUrl,
			MerchantId: merchantId,
			SecretKey: secretKey,
			Token: token);

		await sut.ExecuteAsync(command);

		_ = fakeService
			.Received(1)
			.ExecuteAsync(
			command: Arg.Is(new HandleRecordCommand(
				ExternalRoundId: sessionInfo.Id,
				ExternalGameId: sessionInfo.GameName,
				PlayerCode: sessionInfo.Username,
				ExternalCurrency: sessionInfo.Currency,
				ExternalCreatedOn: sessionInfo.Created.ToDateTimeOffset(),
				ExternalClosedOn: sessionInfo.Closed.ToDateTimeOffset(),
				Bet: sessionStatistics.Bet.ToDecimal().GetValueOrDefault(0.0m),
				Win: sessionStatistics.Win.ToDecimal().GetValueOrDefault(0.0m),
				Games: sessionStatistics.Games,
				Jackpots: sessionStatistics.Jackpots,
				JackpotWin: sessionStatistics.JackpotWin)),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeSessionClient
			.Received(1)
			.UpdateSessionProcessingAsync(
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeSessionClient
			.Received(1)
			.UpdateSessionCompleteAsync(
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeSessionClient
			.Received(0)
			.UpdateSessionRetryAsync(
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SessionInfo為Null()
	{
		var fakeLogger = NullLogger<SessionProcessCommandService>.Instance;
		var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();
		var fakeSessionClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
		var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

		var apiUrl = new Uri("http://localhost/");
		var merchantId = "1234";
		var secretKey = "testSecretKey";
		var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

		var sut = new SessionProcessCommandService(
			logger: fakeLogger,
			client: fakeClient,
			sessionClient: fakeSessionClient,
			service: fakeService);

		var command = new SessionProcessCommand(
			ApiUrl: apiUrl,
			MerchantId: merchantId,
			SecretKey: secretKey,
			Token: token);

		_ = await Assert.ThrowsAsync<ArgumentException>(() => sut.ExecuteAsync(command).AsTask());

		_ = fakeSessionClient
			.Received(1)
			.UpdateSessionProcessingAsync(
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeSessionClient
			.Received(0)
			.UpdateSessionCompleteAsync(
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeSessionClient
			.Received(1)
			.UpdateSessionRetryAsync(
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SessionStatistics為NullAsync()
	{
		var fakeLogger = NullLogger<SessionProcessCommandService>.Instance;
		var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();
		var fakeSessionClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
		var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

		var apiUrl = new Uri("http://localhost/");
		var merchantId = "1234";
		var secretKey = "testSecretKey";
		var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

		var sessionInfo = new GameSessionInfoReply()
		{
			Id = "AA8BB60F5A134264BE838CF6EBA7C8F1",
			GameName = "endorphina_MinotaurDice",
			Username = "4ucmqplxb029",
			Currency = "CNY",
			Created = new DateTimeOffset(2023, 02, 21, 07, 14, 46, TimeSpan.Zero).ToTimestamp(),
			Closed = new DateTimeOffset(2023, 02, 21, 07, 19, 37, TimeSpan.Zero).ToTimestamp(),
			ErrorsPending = 0,
			AccountType = "CREDIT"
		};
		_ = fakeClient.GameSessionInfoAsync(
			apiUrl: Arg.Is(apiUrl),
			merchantId: Arg.Is(merchantId),
			secretKey: Arg.Is(secretKey),
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(sessionInfo);

		var sut = new SessionProcessCommandService(
			logger: fakeLogger,
			client: fakeClient,
			sessionClient: fakeSessionClient,
			service: fakeService);

		var command = new SessionProcessCommand(
			ApiUrl: apiUrl,
			MerchantId: merchantId,
			SecretKey: secretKey,
			Token: token);

		_ = await Assert.ThrowsAsync<ArgumentException>(() => sut.ExecuteAsync(command).AsTask());

		_ = fakeSessionClient
			.Received(1)
			.UpdateSessionProcessingAsync(
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeSessionClient
			.Received(0)
			.UpdateSessionCompleteAsync(
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>());

		_ = fakeSessionClient
			.Received(1)
			.UpdateSessionRetryAsync(
			token: Arg.Is(token),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
