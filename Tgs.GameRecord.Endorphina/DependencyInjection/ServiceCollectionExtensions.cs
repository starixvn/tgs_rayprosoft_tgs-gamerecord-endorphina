using Tgs.GameCommand.Client;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina;

using Tgs.GameRecord.Endorphina.Core;
using Tgs.GameRecord.Endorphina.Session.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecord(
		this IServiceCollection services,
		Action<RedisOptions, IServiceProvider> redisDBconfigure,
		Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
		Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
		Action<GameInterfaceEndorphinaClientOptions, IServiceProvider> gameInterfaceEndorphinaServiceClientOptions,
		Action<GameRecordEndorphinaSessionClientOptions, IServiceProvider> gameRecordEndohphinaSessionClientOptions)
		=> services
		.AddOptions<RedisOptions>()
		.Configure(redisDBconfigure)
		.Services
		.AddOptions<GameRecordInfoOptions>()
		.Configure(gameRecordInfoOptions)
		.Services
		.AddSingleton<RedisConnectionManager>()
		.AddGameRecordCore(
			gameCommandClientOptions: gameCommandClientOptions,
			gameInterfaceEndorphinaServiceClientOptions: gameInterfaceEndorphinaServiceClientOptions,
			gameRecordEndohphinaSessionClientOptions: gameRecordEndohphinaSessionClientOptions,
			redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}
