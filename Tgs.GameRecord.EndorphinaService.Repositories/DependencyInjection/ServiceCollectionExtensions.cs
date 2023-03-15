using StackExchange.Redis;
using Tgs.GameRounds.Work.Core;
using Tgs.GameRounds.Work.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRoundsRepositories(
        this IServiceCollection services,
        Func<IServiceProvider, IDatabase> redisDatabaseResolver)
        => services
        .AddTransient<IBiddingRepository>((sp) => ActivatorUtilities.CreateInstance<BiddingRepository>(sp, redisDatabaseResolver(sp)));
}
