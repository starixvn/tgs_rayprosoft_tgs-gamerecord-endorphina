using Tgs.GameRecord.Endorphina.Core.ApplicationServices;

namespace Tgs.GameRecord.Endorphina.Core;

public interface ISessionQueuedDispatcher
{
	ValueTask PushCommandAsync(SessionQueuedCommand command, CancellationToken cancellationToken = default);
}
