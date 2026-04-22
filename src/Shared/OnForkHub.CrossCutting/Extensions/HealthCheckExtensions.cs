namespace OnForkHub.CrossCutting.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using OnForkHub.CrossCutting.Storage;

public static class HealthCheckExtensions
{
    private static readonly string[] DbTags = { "db", "sql", "ready" };
    private static readonly string[] CacheTags = { "cache", "redis", "ready" };
    private static readonly string[] StorageTags = { "storage", "azure", "ready" };

    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        // 1. SQL Server Health Check
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            hcBuilder.AddSqlServer(connectionString, name: "sql-server", tags: DbTags);
        }

        // 2. Redis Health Check
        var redisEnabled = configuration.GetValue<bool>("Cache:UseRedis");
        var redisConnection = configuration["Cache:RedisConnectionString"];
        if (redisEnabled && !string.IsNullOrEmpty(redisConnection))
        {
            hcBuilder.AddRedis(redisConnection, name: "redis", tags: CacheTags);
        }

        // 3. Azure Storage Health Check
        var storageProvider = configuration["FileStorage:Provider"];
        var azureConnString = configuration["AzureBlobStorage:ConnectionString"];
        if (storageProvider == "Azure" && !string.IsNullOrEmpty(azureConnString))
        {
            hcBuilder.AddAzureBlobStorage(azureConnString, name: "azure-storage", tags: StorageTags);
        }

        return services;
    }
}
