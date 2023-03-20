using Tgs.GameRecord.EndorphinaService.Core.DispatchServices;

namespace Tgs.GameRecord.EndorphinaService.Core;

public interface IScheduleDispatch
{
    public Task<bool> ExistAsync(string key);

    public Task<bool> LockingAsync(string key, ScheduleDispatchCommand command);

    public Task<bool> UnlockAsync(string key);
}
