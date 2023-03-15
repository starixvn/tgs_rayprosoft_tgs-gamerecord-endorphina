using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.EndorphinaService.Core;
using Tgs.GameRounds.Schedule.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEndorphinaServiceCore(
        this IServiceCollection services,
        Action<RedisOptions, IServiceProvider> redisDBconfigure,
        Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameRoundsScheduleClientOptions, IServiceProvider> gameRoundsScheduleClientOptions,
        Action<GameInterfaceEndorphinaClientOptions, IServiceProvider> gameInterfaceEndorphinaServiceClientOptions,
        Action<GameRecordEndorphinaSessionClientOptions, IServiceProvider> gameRecordEndohphinaSessionClientOptions)
        => services
            .AddOptions<RedisOptions>()
            .Configure(redisDBconfigure)
            .Services
            .AddOptions<GameRecordInfoOptions>()
            .Configure(gameRecordInfoOptions)
            .Services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddMemoryCache()
            .AddSingleton<RedisConnectionManager>()
            .AddGameCommandClient(gameCommandClientOptions)
            .AddGameRoundsScheduleClient(gameRoundsScheduleClientOptions)
            .AddGameInterfaceEndorphinaServiceClient(gameInterfaceEndorphinaServiceClientOptions)
            .AddGameRecordEndorphinaSessionClient(gameRecordEndohphinaSessionClientOptions)
            .AddGameRoundsWorkClient()
            .AddGameRoundsWorkManualClient()
            .AddSingleton<ISessionDispatcher, SessionDispatcher>()
            .AddSingleton<ISessionLock>(sp => ActivatorUtilities.CreateInstance<SessionLock>(sp, sp.GetRequiredService<RedisConnectionManager>().Database))
            .AddTransient<IScheduleDispatch>(sp => ActivatorUtilities.CreateInstance<ScheduleDispatch>(sp, sp.GetRequiredService<RedisConnectionManager>().Database))
            .AddTransient<IUtensil>(sp => ActivatorUtilities.CreateInstance<UtensilRepository>(sp, TimeSpan.FromHours(1)));
}
