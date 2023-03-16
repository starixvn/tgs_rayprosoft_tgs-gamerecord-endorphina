using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.GameRecord.EndorphinaService.Core.WorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.UnitTest;

[TestClass]
public class SessionProcessServiceUnitTest
{
    [TestMethod]
    public async Task Normal_Process()
    {
        var fakeService = Substitute.For<ICommandService<WorkCommand>>();
        var fakeSessionClient = Substitute.For<IGameRecordEndorphinaSessionClient>();
        var fakeLogger = NullLogger<SessionProcessCommandService>.Instance;

        var apiUrl = new Uri("http://localhost/");
        var merchantId = "1234";
        var secretKey = "testSecretKey";
        var token = "testToken";

        var sut = new SessionProcessCommandService(
            fakeService,
            fakeSessionClient,
            fakeLogger);

        var command = new SessionProcessCommand(
            ApiUrl: apiUrl,
            MerchantId: merchantId,
            SecretKey: secretKey,
            Token: token);

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakeSessionClient
            .Received(1)
            .UpdateSessionProcessingAsync(token, Arg.Any<CancellationToken>());

        _ = fakeService
            .Received(1)
            .ExecuteAsync(
                Arg.Any<WorkCommand>(),
                Arg.Any<CancellationToken>());

        _ = fakeSessionClient
            .Received(1)
            .UpdateSessionCompleteAsync(token, Arg.Any<CancellationToken>());
    }
}
