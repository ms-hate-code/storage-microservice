using BuildingBlocks.Web;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BuildingBlocks.Caching;

public static class Extension
{
    public static IServiceCollection AddCustomRedisCaching(this IServiceCollection services)
    {
        var redisOptions = services.GetOptions<RedisOptions>(nameof(RedisOptions));
        services.AddValidateOptions<RedisOptions>();

        var endpoints = new EndPointCollection { { redisOptions.Host, redisOptions.Port } };
        var configuration = new ConfigurationOptions()
        {
            SyncTimeout = 500000,
            EndPoints = endpoints,
        };  

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration));
        services.AddScoped<ICachingHandlerService, CachingHandlerService>();

        return services;
    }
}