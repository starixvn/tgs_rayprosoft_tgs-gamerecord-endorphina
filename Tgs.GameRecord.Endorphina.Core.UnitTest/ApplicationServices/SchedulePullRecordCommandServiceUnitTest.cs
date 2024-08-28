using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameRecord.Endorphina.Common;
using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.Endorphina.Session.Protos;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.UnitTest.ApplicationServices;

public class SchedulePullRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Normal_Process()
	{
		var fakeLogger = NullLogger<SchedulePullRecordCommandService>.Instance;
		var fakeClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var apiUrl = new Uri("http://localhost/");
		var merchantId = "1234";
		var secretKey = "testSecretKey";

		var session = new Session.Protos.Session
		{
			Token = "AA8BB60F5A134264BE838CF6EBA7C8F1"
		};
		_ = fakeClient.QuerySessionsAsync(
			state: Arg.Is(TokenState.Queued),
			merchantId: Arg.Is(merchantId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(new[] { session }.ToAsyncEnumerable());

		var sut = new SchedulePullRecordCommandService(
			fakeLogger,
			fakeClient,
			fakeMessageSender);

		var command = new SchedulePullRecordCommand(
			ApiUrl: apiUrl,
			MerchantId: merchantId,
			SecretKey: secretKey);

		await sut.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(EndorphinaNatsConst.SessionQueuedPublish),
			data: Arg.Is(new Common.Protos.SessionProcess
			{
				ApiUrl = command.ApiUrl.ToString(),
				MerchantId = command.MerchantId,
				SecretKey = command.SecretKey,
				Token = session.Token
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task No_Data()
	{
		var fakeLogger = NullLogger<SchedulePullRecordCommandService>.Instance;
		var fakeClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var apiUrl = new Uri("http://localhost/");
		var merchantId = "1234";
		var secretKey = "testSecretKey";

		_ = fakeClient.QuerySessionsAsync(
			state: Arg.Is(TokenState.Queued),
			merchantId: Arg.Is(merchantId),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(Enumerable.Empty<Session.Protos.Session>().ToAsyncEnumerable());

		var sut = new SchedulePullRecordCommandService(
			fakeLogger,
			fakeClient,
			fakeMessageSender);

		var command = new SchedulePullRecordCommand(
			ApiUrl: apiUrl,
			MerchantId: merchantId,
			SecretKey: secretKey);

		await sut.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(0)
			.PublishAsync(
			subject: Arg.Is(EndorphinaNatsConst.SessionQueuedPublish),
			data: Arg.Any<Common.Protos.SessionProcess>(),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
