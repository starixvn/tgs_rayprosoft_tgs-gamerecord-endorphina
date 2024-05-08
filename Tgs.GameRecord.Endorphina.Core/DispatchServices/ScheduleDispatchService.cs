using Microsoft.Extensions.Options;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.DispatchServices;

internal class ScheduleDispatchService(
	IOptions<GameRecordInfoOptions> options,
	ISessionDispatcher sessionDispatcher,
	IScheduleDispatch scheduleDispatch) : ICommandService<ScheduleDispatchCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfoOptions = options.Value;

	public async ValueTask ExecuteAsync(ScheduleDispatchCommand command, CancellationToken cancellationToken = default)
	{
		var key = $"gp:gamerecord:{_gameRecordInfoOptions.GameProviderCode}:{command.MerchantId}".ToLower();
		if (!await scheduleDispatch.ExistAsync(key: key).ConfigureAwait(false))
		{
			if (await scheduleDispatch.LockingAsync(key: key, command: command).ConfigureAwait(false))
			{
				try
				{
					await sessionDispatcher
						.ExecuteAsync(
							apiUrl: command.ApiUrl,
							merchantId: command.MerchantId,
							secretKey: command.SecretKey,
							cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				finally
				{
					_ = await scheduleDispatch.UnlockAsync(key: key).ConfigureAwait(false);
				}
			}
		}
	}
}
