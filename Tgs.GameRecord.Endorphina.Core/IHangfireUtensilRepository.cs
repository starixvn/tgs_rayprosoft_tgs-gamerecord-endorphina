using Tgs.GameRecord.Endorphina.Core.JobApplicationServices;

namespace Tgs.GameRecord.Endorphina.Core;

public interface IHangfireUtensilRepository
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
