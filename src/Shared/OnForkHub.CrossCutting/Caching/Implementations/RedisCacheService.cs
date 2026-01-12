namespace OnForkHub.CrossCutting.Caching.Implementations;

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

/// <summary>
/// Redis implementation of the cache service.
/// Suitable for production and multi-instance deployments.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };

    private readonly IDistributedCache _cache;
    private readonly CacheOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache instance.</param>
    /// <param name="options">The cache options.</param>
    public RedisCacheService(IDistributedCache cache, IOptions<CacheOptions> options)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(key);

        var fullKey = GetFullKey(key);
        var data = await _cache.GetStringAsync(fullKey, cancellationToken);

        if (string.IsNullOrEmpty(data))
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(data, JsonOptions);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        var fullKey = GetFullKey(key);
        var data = JsonSerializer.Serialize(value, JsonOptions);

        var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };

        await _cache.SetStringAsync(fullKey, data, cacheOptions, cancellationToken);
    }

    /// <inheritdoc/>
    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        where T : class
    {
        return SetAsync(key, value, _options.DefaultExpiration, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var fullKey = GetFullKey(key);
        await _cache.RemoveAsync(fullKey, cancellationToken);
    }

    /// <inheritdoc/>
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Pattern-based removal requires direct Redis connection
        // This is a simplified implementation that removes the exact key
        // For full pattern support, consider using StackExchange.Redis directly
        return RemoveAsync(pattern, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);

        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var fullKey = GetFullKey(key);
        var data = await _cache.GetStringAsync(fullKey, cancellationToken);
        return !string.IsNullOrEmpty(data);
    }

    private string GetFullKey(string key) => $"{_options.InstanceName}{key}";
}
