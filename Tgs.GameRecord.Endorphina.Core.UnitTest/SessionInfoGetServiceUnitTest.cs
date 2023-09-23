using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameInterface.EndorphinaService.Protos;
using Tgs.GameRecord.Endorphina.Core.ApplicationServices;

namespace Tgs.GameRecord.Endorphina.Core.UnitTest;

[TestClass]
public class SessionInfoGetServiceUnitTest
{
    [TestMethod]
    public async Task Normal_Process()
    {
        var fakeClient = Substitute.For<IGameInterfaceEndorphinaClient>();
        var fakeLogger = NullLogger<SessionInfoGetService>.Instance;

        var apiUrl = new Uri("http://localhost/");
        var merchantId = "1234";
        var secretKey = "testSecretKey";
        var token = "AA8BB60F5A134264BE838CF6EBA7C8F1";

        var reply = new GameSessionInfoReply()
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
            .Returns(reply);

        var sut = new SessionInfoGetService(fakeClient, fakeLogger);

        var actual = await sut.GetAsync(new SessionInfoQuery(apiUrl, merchantId, secretKey, token));

        Assert.IsNotNull(actual);
    }
}
