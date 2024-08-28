using Tgs.GameRecord.Endorphina.Core.ApplicationServices;

namespace Tgs.GameRecord.Endorphina.Core;

public interface IManualPullRecordDispatcher
{
	ValueTask PushCommandAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default);
}
