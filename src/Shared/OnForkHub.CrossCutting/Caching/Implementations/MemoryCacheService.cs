namespace OnForkHub.CrossCutting.Caching.Implementations;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;

/// <summary>
/// In-memory implementation of the cache service.
/// Suitable for development and single-instance deployments.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheService"/> class.
    /// </summary>
    /// <param name="cache">The memory cache instance.</param>
    /// <param name="options">The cache options.</param>
    public MemoryCacheService(IMemoryCache cache, IOptions<CacheOptions> options)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(key);

        var fullKey = GetFullKey(key);
        var result = _cache.Get<T>(fullKey);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        var fullKey = GetFullKey(key);
        var cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };

        _cache.Set(fullKey, value, cacheOptions);
        _keys.TryAdd(fullKey, 0);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        where T : class
    {
        return SetAsync(key, value, _options.DefaultExpiration, cancellationToken);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var fullKey = GetFullKey(key);
        _cache.Remove(fullKey);
        _keys.TryRemove(fullKey, out _);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        var fullPattern = GetFullKey(pattern);
        var keysToRemove = _keys.Keys.Where(k => k.StartsWith(fullPattern, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
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
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var fullKey = GetFullKey(key);
        var exists = _cache.TryGetValue(fullKey, out _);
        return Task.FromResult(exists);
    }

    private string GetFullKey(string key) => $"{_options.InstanceName}{key}";
}
