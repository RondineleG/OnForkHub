namespace OnForkHub.CrossCutting.Caching;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OnForkHub.CrossCutting.Caching.Implementations;

/// <summary>
/// Extension methods for registering caching services.
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Adds caching services to the dependency injection container.
    /// Automatically selects Redis or Memory cache based on configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var cacheOptions = new CacheOptions();
        configuration.GetSection(CacheOptions.SectionName).Bind(cacheOptions);

        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        if (cacheOptions.UseRedis && !string.IsNullOrEmpty(cacheOptions.RedisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheOptions.RedisConnectionString;
                options.InstanceName = cacheOptions.InstanceName;
            });

            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        return services;
    }

    /// <summary>
    /// Adds in-memory caching services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddMemoryCachingServices(this IServiceCollection services)
    {
        services.Configure<CacheOptions>(_ => { });
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }

    /// <summary>
    /// Adds Redis caching services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The Redis connection string.</param>
    /// <param name="instanceName">The instance name prefix.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddRedisCachingServices(
        this IServiceCollection services,
        string connectionString,
        string instanceName = "OnForkHub_"
    )
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        services.Configure<CacheOptions>(options =>
        {
            options.UseRedis = true;
            options.RedisConnectionString = connectionString;
            options.InstanceName = instanceName;
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = instanceName;
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
