using StackExchange.Redis;
using Tgs.GameRecord.EndorphinaService.Core;

namespace Tgs.GameRecord.EndorphinaService.Repositories;

internal class MutexRepository : IMutexRepository
{
    private readonly IDatabase _database;

    public MutexRepository(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public Task<bool> LockAsync(string token)
        => _database.StringSetAsync(key: $"endor:token:{token}", value: token);

    public Task<bool> UnlockAsync(string token)
        => _database.KeyDeleteAsync(key: $"endor:token:{token}");
}