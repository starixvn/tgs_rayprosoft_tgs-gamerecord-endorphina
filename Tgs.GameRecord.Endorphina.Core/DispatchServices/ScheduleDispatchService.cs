using Microsoft.Extensions.Options;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.DispatchServices;

internal class ScheduleDispatchService : ICommandService<ScheduleDispatchCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;
    private readonly ISessionDispatcher _sessionDispatcher;
    private readonly IScheduleDispatch _scheduleDispatch;

    public ScheduleDispatchService(
        IOptions<GameRecordInfoOptions> options,
        ISessionDispatcher sessionDispatcher,
        IScheduleDispatch scheduleDispatch)
    {
        _gameRecordInfoOptions = options.Value;
        _sessionDispatcher = sessionDispatcher ?? throw new ArgumentNullException(nameof(sessionDispatcher));
        _scheduleDispatch = scheduleDispatch ?? throw new ArgumentNullException(nameof(scheduleDispatch));
    }

    public async ValueTask ExecuteAsync(ScheduleDispatchCommand command, CancellationToken cancellationToken = default)
    {
        var key = $"gp:gamerecord:{_gameRecordInfoOptions.GameProviderCode}:{command.MerchantId}".ToLower();
        if (!await _scheduleDispatch.ExistAsync(key: key).ConfigureAwait(false))
        {
            if (await _scheduleDispatch.LockingAsync(key: key, command: command).ConfigureAwait(false))
            {
                try
                {
                    await _sessionDispatcher
                        .ExecuteAsync(
                            apiUrl: command.ApiUrl,
                            merchantId: command.MerchantId,
                            secretKey: command.SecretKey,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    _ = await _scheduleDispatch.UnlockAsync(key: key).ConfigureAwait(false);
                }
            }
        }
    }
}
