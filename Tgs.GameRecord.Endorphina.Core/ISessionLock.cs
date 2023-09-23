namespace Tgs.GameRecord.Endorphina.Core;

public interface ISessionLock
{
    Task<bool> LockAsync(
        string token);

    Task<bool> UnlockAsync(
        string token);
}
