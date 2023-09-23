using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Core;
using Tgs.GameRecord.Endorphina.Session.Client;
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
        .AddSingleton<RedisConnectionManager>()
        .AddGameCommandClient(gameCommandClientOptions)
        .AddGameRoundsScheduleClient(gameRoundsScheduleClientOptions)
        .AddGameInterfaceEndorphinaServiceClient(gameInterfaceEndorphinaServiceClientOptions)
        .AddGameRecordEndorphinaSessionClient(gameRecordEndohphinaSessionClientOptions)
        .AddGameRecordHandleClient()
        .AddTransient<IHangfireUtensil, HangfireUtensil>()
        .AddSingleton<ISessionDispatcher, SessionDispatcher>()
        .AddSingleton<ISessionLock>(sp => ActivatorUtilities.CreateInstance<SessionLock>(sp, sp.GetRequiredService<RedisConnectionManager>().Database))
        .AddTransient<IScheduleDispatch>(sp => ActivatorUtilities.CreateInstance<ScheduleDispatch>(sp, sp.GetRequiredService<RedisConnectionManager>().Database));
}
