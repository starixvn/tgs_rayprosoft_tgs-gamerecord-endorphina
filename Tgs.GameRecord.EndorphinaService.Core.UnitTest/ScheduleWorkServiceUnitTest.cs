using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameRecord.Endorphina.Session.Protos;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.GameRecord.EndorphinaService.Core.ScheduleWorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.UnitTest;

[TestClass]
public class ScheduleWorkServiceUnitTest
{
    [TestMethod]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<ScheduleWorkerService>.Instance;
        var fakePullRecordCommandService = Substitute.For<ICommandService<GamePullRecordCommand>>();

        var sut = new ScheduleWorkerService(fakeLogger, fakePullRecordCommandService);

        var command = new ScheduleCommand(
            ApiUrl: new Uri("http://localhost/"),
            MerchantId: "1234",
            SecretKey: "testSecretKey",
            Session: new Session()
            {
                BrandCode = "GPI",
                PlayerCode = "4ucmqplxb029",
                Retry = 0,
                State = TokenState.Queued,
                Token = "testToken"
            });

        await sut.ExecuteAsync(command).ConfigureAwait(false);

        _ = fakePullRecordCommandService
            .Received(1)
            .ExecuteAsync(
                new GamePullRecordCommand(command.ApiUrl, command.MerchantId, command.SecretKey, command.Session));
    }
}
