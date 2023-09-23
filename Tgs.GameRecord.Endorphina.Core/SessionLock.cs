using StackExchange.Redis;

namespace Tgs.GameRecord.Endorphina.Core;

internal class SessionLock : ISessionLock
{
    private readonly IDatabase _database;

    public SessionLock(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public Task<bool> LockAsync(string token)
        => _database.StringSetAsync(key: $"endor:token:{token}", value: token, expiry: TimeSpan.FromMinutes(1), when: When.NotExists);

    public Task<bool> UnlockAsync(string token)
        => _database.KeyDeleteAsync(key: $"endor:token:{token}");
}
