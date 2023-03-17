using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameInterface.EndorphinaService.Protos;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.UnitTest;

[TestClass]
public class ScheduleSessionProcessCommandServiceUnitTest
{
    [TestMethod]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<ScheduleSessionProcessCommandService>.Instance;
        var fakeService = Substitute.For<ICommandService<GamePullRecordCommand>>();
        var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();
        var fakeSessionClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
        var fakeSessionLock = Substitute.For<ISessionLock>();

        var apiUrl = new Uri("http://localhost/");
        var merchantId = "1234";
        var secretKey = "testSecretKey";
        var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

        _ = fakeSessionLock
            .LockAsync(token)
            .Returns(true);

        var sessionInfoReply = new GameSessionInfoReply()
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

        _ = fakeClient
            .GameSessionInfoAsync(apiUrl, merchantId, secretKey, token)
            .Returns(sessionInfoReply);

        var statistcsReply = new GameSessionStatisticsReply()
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

        _ = fakeClient
            .GameSessionStatisticsAsync(apiUrl, merchantId, secretKey, token)
            .Returns(statistcsReply);

        var sut = new ScheduleSessionProcessCommandService(fakeLogger, fakeClient, fakeSessionClient, fakeSessionLock, fakeService);

        var command = new ScheduleSessionProcessCommand(apiUrl, merchantId, secretKey, token);
        {
        };

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeService
            .Received(1)
            .ExecuteAsync(Arg.Any<GamePullRecordCommand>());

        _ = fakeSessionClient
            .Received(1)
            .UpdateSessionCompleteAsync(token);

        _ = fakeSessionLock.Received(1)
            .UnlockAsync(token);
    }

    [TestMethod]
    public void SessionInfo為Null()
    {
        var fakeLogger = NullLogger<ScheduleSessionProcessCommandService>.Instance;        
        var fakeService = Substitute.For<ICommandService<GamePullRecordCommand>>();
        var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();
        var fakeSessionClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
        var fakeSessionLock = Substitute.For<ISessionLock>();

        var apiUrl = new Uri("http://localhost/");
        var merchantId = "1234";
        var secretKey = "testSecretKey";
        var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

        _ = fakeSessionLock
            .LockAsync(token)
            .Returns(true);

        var sut = new ScheduleSessionProcessCommandService(fakeLogger, fakeClient, fakeSessionClient, fakeSessionLock, fakeService);

        var command = new ScheduleSessionProcessCommand(apiUrl, merchantId, secretKey, token);

        _ = Assert.ThrowsExceptionAsync<ArgumentException>(async () => await sut.ExecuteAsync(command).ConfigureAwait(false));
    }

    [TestMethod]
    public void SessionStatistics為Null()
    {
        var fakeLogger = NullLogger<ScheduleSessionProcessCommandService>.Instance;        
        var fakeService = Substitute.For<ICommandService<GamePullRecordCommand>>();
        var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();
        var fakeSessionClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
        var fakeSessionLock = Substitute.For<ISessionLock>();

        var apiUrl = new Uri("http://localhost/");
        var merchantId = "1234";
        var secretKey = "testSecretKey";
        var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

        _ = fakeSessionLock
            .LockAsync(token)
            .Returns(true);

        var sessionInfoReply = new GameSessionInfoReply()
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

        _ = fakeClient
            .GameSessionInfoAsync(apiUrl, merchantId, secretKey, token)
            .Returns(sessionInfoReply);

        var sut = new ScheduleSessionProcessCommandService(fakeLogger, fakeClient, fakeSessionClient, fakeSessionLock, fakeService);

        var command = new ScheduleSessionProcessCommand(apiUrl, merchantId, secretKey, token);

        _ = Assert.ThrowsExceptionAsync<ArgumentException>(async () => await sut.ExecuteAsync(command).ConfigureAwait(false));
    }

    [TestMethod]
    public async Task SessionLock為False不繼續執行()
    {
        var fakeLogger = NullLogger<ScheduleSessionProcessCommandService>.Instance;        
        var fakeService = Substitute.For<ICommandService<GamePullRecordCommand>>();
        var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();
        var fakeSessionClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
        var fakeSessionLock = Substitute.For<ISessionLock>();

        var apiUrl = new Uri("http://localhost/");
        var merchantId = "1234";
        var secretKey = "testSecretKey";
        var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

        _ = fakeSessionLock
            .LockAsync(token)
            .Returns(false);

        var sut = new ScheduleSessionProcessCommandService(
            fakeLogger,
            fakeClient,
            fakeSessionClient,
            fakeSessionLock,
            fakeService);

        var command = new ScheduleSessionProcessCommand(
            ApiUrl: apiUrl,
            MerchantId: merchantId,
            SecretKey: secretKey,
            Token: token);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeSessionClient
           .Received(0)
           .UpdateSessionProcessingAsync(token);
    }
}
