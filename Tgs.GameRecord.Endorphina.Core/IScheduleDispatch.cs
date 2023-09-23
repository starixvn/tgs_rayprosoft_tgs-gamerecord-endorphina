using Tgs.GameRecord.Endorphina.Core.DispatchServices;

namespace Tgs.GameRecord.Endorphina.Core;

public interface IScheduleDispatch
{
    public Task<bool> ExistAsync(string key);

    public Task<bool> LockingAsync(string key, ScheduleDispatchCommand command);

    public Task<bool> UnlockAsync(string key);
}
