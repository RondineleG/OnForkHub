namespace OnForkHub.CrossCutting.Caching;

/// <summary>
/// Configuration options for caching services.
/// </summary>
public sealed class CacheOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Cache";

    /// <summary>
    /// Gets or sets the default expiration time in minutes.
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Gets or sets the Redis connection string.
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Redis is enabled.
    /// </summary>
    public bool UseRedis { get; set; }

    /// <summary>
    /// Gets or sets the instance name prefix for cache keys.
    /// </summary>
    public string InstanceName { get; set; } = "OnForkHub_";

    /// <summary>
    /// Gets the default expiration as TimeSpan.
    /// </summary>
    public TimeSpan DefaultExpiration => TimeSpan.FromMinutes(DefaultExpirationMinutes);
}
