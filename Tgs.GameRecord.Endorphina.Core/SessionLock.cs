using StackExchange.Redis;

namespace Tgs.GameRecord.Endorphina.Core;

internal class SessionLock(IDatabase database) : ISessionLock
{
    public Task<bool> LockAsync(string token)
        => database.StringSetAsync(key: $"endor:token:{token}", value: token, expiry: TimeSpan.FromMinutes(1), when: When.NotExists);

    public Task<bool> UnlockAsync(string token)
        => database.KeyDeleteAsync(key: $"endor:token:{token}");
}
