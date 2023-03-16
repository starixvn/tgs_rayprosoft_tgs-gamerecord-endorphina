using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameInterface.EndorphinaService.Protos;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.GameRecord.EndorphinaService.Core.WorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.UnitTest;

[TestClass]
public class WorkServiceUnitTest
{
    [TestMethod]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<WorkerService>.Instance;
        var fakeService = Substitute.For<ICommandService<GamePullRecordCommand>>();
        var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();
        var fakeSessionClient = Substitute.For<IGameRecordEndorphinaSessionClient>();

        var apiUrl = new Uri("http://localhost/");
        var merchantId = "1234";
        var secretKey = "testSecretKey";
        var token = "testToken";

        var sessionReply = new GameSessionInfoReply()
        {
            Id = "AA8BB60F5A134264BE838CF6EBA7C8F1",
            Created = new DateTimeOffset(2023, 02, 21, 07, 14, 46, TimeSpan.Zero).ToTimestamp(),
            Closed = new DateTimeOffset(2023, 02, 21, 07, 19, 37, TimeSpan.Zero).ToTimestamp(),
            Currency = "CNY",
            AccountType = "CREDIT",
            GameName = "endorphina_MinotaurDice",
            Username = "4ucmqplxb029",
            ErrorsPending = 0
        };

        _ = fakeClient
            .GameSessionInfoAsync(apiUrl, merchantId, secretKey, token)
            .Returns(sessionReply);

        var statisticsReply = new GameSessionStatisticsReply()
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
            .Returns(statisticsReply);

        var sut = new WorkerService(
            fakeLogger,
            fakeClient,
            fakeSessionClient,
            fakeService);

        var command = new WorkCommand(
            ApiUrl: apiUrl,
            MerchantId: merchantId,
            SecretKey: secretKey,
            Token: token);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeService
            .Received(1)
            .ExecuteAsync(
                Arg.Any<GamePullRecordCommand>());
    }
}
