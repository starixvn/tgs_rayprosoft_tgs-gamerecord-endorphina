namespace Tgs.GameRecord.EndorphinaService.Core;

public interface ISessionLock
{
    Task<bool> LockAsync(
        string token);

    Task<bool> UnlockAsync(
        string token);
}
