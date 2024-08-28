using Tgs.GameRecord.Endorphina.Core.ApplicationServices;

namespace Tgs.GameRecord.Endorphina.Core;

public interface ISessionProcessDispatcher
{
	ValueTask PushCommandAsync(SessionProcessCommand command, CancellationToken cancellationToken = default);
}
