using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameRecord.Endorphina.Session.Protos;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.GameRecord.EndorphinaService.Core.ManualWorkerServices;
using Tgs.GameRecord.EndorphinaService.Core.ScheduleWorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.UnitTest;

[TestClass]
public class ManualWorkServiceUnitTest
{
    [TestMethod]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<ScheduleWorkerService>.Instance;
        var fakePullRecordCommandService = Substitute.For<ICommandService<GamePullRecordCommand>>();

        var sut = new ManualWorkerService(fakeLogger, fakePullRecordCommandService);

        var command = new ManualCommand(
            ApiUrl: new Uri("http://localhost/"),
            MerchantId: "1234",
            SecretKey: "testSecretKey",
            Token: "testToken",
            PlayerCode: "4ucmqplxb029");

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakePullRecordCommandService
            .Received(1)
            .ExecuteAsync(
                new GamePullRecordCommand(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    new Session()
                    {
                        Token = command.Token,
                        PlayerCode = command.PlayerCode,
                        Retry = 0,
                        BrandCode = "",
                        State = TokenState.Processing
                    }));
    }
}
