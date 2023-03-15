using Microsoft.Extensions.Logging;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ScheduleWorkerServices;

internal class ScheduleWorkerService : ICommandService<ScheduleCommand>
{
    private readonly ILogger<ScheduleWorkerService> _logger;
    private readonly ICommandService<GamePullRecordCommand> _service;

    public ScheduleWorkerService(
        ILogger<ScheduleWorkerService> logger,
        ICommandService<GamePullRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(ScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var recordCommand = new GamePullRecordCommand(
            ApiUrl: command.ApiUrl,
            MerchantId: command.MerchantId,
            SecretKey: command.SecretKey,
            Session: command.Session);

        await _service
            .ExecuteAsync(recordCommand, cancellationToken)
            .ConfigureAwait(false);
    }
}
