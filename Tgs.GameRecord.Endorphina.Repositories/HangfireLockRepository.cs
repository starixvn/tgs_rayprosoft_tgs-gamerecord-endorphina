using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tgs.GameRecord.Endorphina.Core;

namespace Tgs.GameRecord.Endorphina.Repositories;
internal class HangfireLockRepository(
	IOptions<GameRecordInfoOptions> options,
	IDatabase database) : IHangfireLockRepository
{
	private readonly GameRecordInfoOptions _options = options.Value;

	public Task<bool> LockingAsync(string application, CancellationToken cancellationToken = default)
			=> database.StringSetAsync(
			key: GetKey(application),
			value: new JsonObject
			{
				["MachineName"] = Environment.MachineName,
				["Application"] = application,
				["LabelTime"] = DateTimeOffset.Now
			}.ToJsonString(),
			expiry: TimeSpan.FromSeconds(59),
			when: When.NotExists);

	private string GetKey(string application)
		=> string.Concat(_options.HangfireLocking, application);
}
