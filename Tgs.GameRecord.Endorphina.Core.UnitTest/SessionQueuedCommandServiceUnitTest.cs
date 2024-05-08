using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameInterface.EndorphinaService.Protos;
using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Tgs.GameRecord.Endorphina.Core.Protos;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.UnitTest;

[TestClass]
public class SessionQueuedCommandServiceUnitTest
{
	[TestMethod]
	public async Task Normal_Process()
	{
		var fakeLogger = NullLogger<ScheduleSessionQueuedCommandService>.Instance;
		var fakeMessageSender = Substitute.For<IMessageSender>();
		var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();

		var sut = new ScheduleSessionQueuedCommandService(
			fakeLogger,
			fakeClient,
			fakeMessageSender);

		var apiUrl = new Uri("http://localhost/");
		var merchantId = "1234";
		var secretKey = "testSecretKey";
		var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

		_ = fakeClient.GameSessionInfoAsync(apiUrl, merchantId, secretKey, token)
			.Returns(new GameSessionInfoReply()
			{
				Id = token,
				Created = new DateTimeOffset(2023, 02, 21, 07, 14, 46, TimeSpan.Zero).ToTimestamp(),
				Closed = new DateTimeOffset(2023, 02, 21, 07, 19, 37, TimeSpan.Zero).ToTimestamp(),
				Currency = "CNY",
				AccountType = "CREDIT",
				GameName = "endorphina_MinotaurDice",
				Username = "4ucmqplxb029",
				ErrorsPending = 0
			});

		await sut.ExecuteAsync(new ScheduleSessionQueuedCommand(
			ApiUrl: apiUrl,
			MerchantId: merchantId,
			SecretKey: secretKey,
			Token: token));

		_ = fakeMessageSender.Received(1)
			.PublishAsync(
				"gp.gamerecord.endor.session.process",
				Arg.Any<ReadOnlyMemory<byte>>());
	}

	[TestMethod]
	public async Task SessionInfo為Null時不繼續執行()
	{
		var fakeLogger = NullLogger<ScheduleSessionQueuedCommandService>.Instance;
		var fakeMessageSender = Substitute.For<IMessageSender>();
		var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();

		var sut = new ScheduleSessionQueuedCommandService(
			fakeLogger,
			fakeClient,
			fakeMessageSender);

		var apiUrl = new Uri("http://localhost/");
		var merchantId = "1234";
		var secretKey = "testSecretKey";
		var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

		await sut.ExecuteAsync(new ScheduleSessionQueuedCommand(
			ApiUrl: apiUrl,
			MerchantId: merchantId,
			SecretKey: secretKey,
			Token: token));

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
				"gp.gamerecord.endor.session.process",
				new SessionProcess()
				{
					ApiUrl = apiUrl.ToString(),
					MerchantId = merchantId,
					SecretKey = secretKey,
					Token = token
				}.ToByteArray());
	}

	[TestMethod]
	public async Task SessionInfo的ErrorsPending不為0時不繼續執行()
	{
		var fakeLogger = NullLogger<ScheduleSessionQueuedCommandService>.Instance;
		var fakeMessageSender = Substitute.For<IMessageSender>();
		var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();

		var apiUrl = new Uri("http://localhost/");
		var merchantId = "1234";
		var secretKey = "testSecretKey";
		var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

		_ = fakeClient.GameSessionInfoAsync(apiUrl, merchantId, secretKey, token)
			.Returns(new GameSessionInfoReply()
			{
				Id = token,
				Created = new DateTimeOffset(2023, 02, 21, 07, 14, 46, TimeSpan.Zero).ToTimestamp(),
				Closed = new DateTimeOffset(2023, 02, 21, 07, 19, 37, TimeSpan.Zero).ToTimestamp(),
				Currency = "CNY",
				AccountType = "CREDIT",
				GameName = "endorphina_MinotaurDice",
				Username = "4ucmqplxb029",
				ErrorsPending = 2
			});

		var sut = new ScheduleSessionQueuedCommandService(
			fakeLogger,
			fakeClient,
			fakeMessageSender);

		await sut.ExecuteAsync(new ScheduleSessionQueuedCommand(
			ApiUrl: apiUrl,
			MerchantId: merchantId,
			SecretKey: secretKey,
			Token: token));

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
				"gp.gamerecord.endor.session.process",
				new SessionProcess()
				{
					ApiUrl = apiUrl.ToString(),
					MerchantId = merchantId,
					SecretKey = secretKey,
					Token = token
				}.ToByteArray());
	}
}
