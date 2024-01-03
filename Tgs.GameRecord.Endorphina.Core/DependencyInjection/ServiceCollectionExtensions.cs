using System.Reflection;
using StackExchange.Redis;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Core;
using Tgs.GameRecord.Endorphina.Session.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordCore(
        this IServiceCollection services,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameInterfaceEndorphinaClientOptions, IServiceProvider> gameInterfaceEndorphinaServiceClientOptions,
        Action<GameRecordEndorphinaSessionClientOptions, IServiceProvider> gameRecordEndohphinaSessionClientOptions,
         Func<IServiceProvider, IDatabase> redisDatabaseResolver)
        => services
        .RegisterAllServiceUtilityTypes(
            Assembly.GetExecutingAssembly(),
            ServiceLifetime.Transient)
        .AddGameCommandClient(gameCommandClientOptions)
        .AddGameInterfaceEndorphinaServiceClient(gameInterfaceEndorphinaServiceClientOptions)
        .AddGameRecordEndorphinaSessionClient(gameRecordEndohphinaSessionClientOptions)
        .AddGameRecordHandleClient()
        .AddTransient<IHangfireUtensil, HangfireUtensil>()
        .AddSingleton<ISessionDispatcher, SessionDispatcher>()
        .AddSingleton<ISessionLock>(sp => ActivatorUtilities.CreateInstance<SessionLock>(sp, redisDatabaseResolver(sp)))
        .AddTransient<IScheduleDispatch>(sp => ActivatorUtilities.CreateInstance<ScheduleDispatch>(sp, redisDatabaseResolver(sp)));
}
