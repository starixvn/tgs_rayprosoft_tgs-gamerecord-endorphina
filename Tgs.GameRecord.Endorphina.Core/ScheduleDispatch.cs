using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tgs.GameRecord.Endorphina.Core.DispatchServices;

namespace Tgs.GameRecord.Endorphina.Core;

internal class ScheduleDispatch : IScheduleDispatch
{
	private readonly IDatabase _database;
	private readonly TimeSpan _expiry;
	private readonly GameRecordInfoOptions _gameRecordInfoOptions;

	public ScheduleDispatch(
		IOptions<GameRecordInfoOptions> options,
		IDatabase database)
	{
		_gameRecordInfoOptions = options.Value;
		_database = database ?? throw new ArgumentNullException(nameof(database));
		_expiry = TimeSpan.FromMinutes(_gameRecordInfoOptions.LockMinute);
	}

	public Task<bool> ExistAsync(string key)
		=> _database.KeyExistsAsync(key: key);

	public Task<bool> LockingAsync(string key, ScheduleDispatchCommand command)
		=> _database.StringSetAsync(key: key, value: JsonSerializer.Serialize(command), expiry: _expiry, when: When.NotExists);

	public Task<bool> UnlockAsync(string key)
		=> _database.KeyDeleteAsync(key: key);
}
