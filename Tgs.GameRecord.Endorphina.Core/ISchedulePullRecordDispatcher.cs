using Tgs.GameRecord.Endorphina.Core.ApplicationServices;

namespace Tgs.GameRecord.Endorphina.Core;

public interface ISchedulePullRecordDispatcher
{
	ValueTask PushCommandAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default);
}
