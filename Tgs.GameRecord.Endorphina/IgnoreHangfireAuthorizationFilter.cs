using Hangfire.Dashboard;

namespace Tgs.GameRecord.Endorphina;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
