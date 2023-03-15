using Microsoft.Extensions.Logging;
using Tgs.GameRecord.Endorphina.Session.Protos;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.GameRecord.EndorphinaService.Core.ScheduleWorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ManualWorkerServices;

internal class ManualWorkerService : ICommandService<ManualCommand>
{
    private readonly ILogger<ScheduleWorkerService> _logger;
    private readonly ICommandService<GamePullRecordCommand> _pullRecordCommandService;

    public ManualWorkerService(
        ILogger<ScheduleWorkerService> logger,
        ICommandService<GamePullRecordCommand> pullRecordCommandService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pullRecordCommandService = pullRecordCommandService ?? throw new ArgumentNullException(nameof(pullRecordCommandService));
    }

    public async ValueTask ExecuteAsync(ManualCommand command, CancellationToken cancellationToken = default)
    {
        var recordCommand = new GamePullRecordCommand(
            ApiUrl: command.ApiUrl,
            MerchantId: command.MerchantId,
            SecretKey: command.SecretKey,
            Session: new Session()
            {
                State = TokenState.Processing,
                PlayerCode = command.PlayerCode,
                Token = command.Token,
                Retry = 0,
                BrandCode = string.Empty
            });

        await _pullRecordCommandService
            .ExecuteAsync(recordCommand, cancellationToken)
            .ConfigureAwait(false);
    }
}
