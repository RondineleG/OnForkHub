using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace OnForkHub.Application.DependencyInjection;

internal sealed class ConcurrentCache<TKey, TValue> : IDisposable
    where TKey : notnull
{
    public ConcurrentCache(int maxSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);
        _maxSize = maxSize;
        _cache = new ConcurrentDictionary<TKey, CacheEntry>();
    }

    private readonly ConcurrentDictionary<TKey, CacheEntry> _cache;

    private readonly int _maxSize;

    private volatile bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            _cache.Clear();
            _disposed = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cache.TryGetValue(key, out var entry))
        {
            entry.UpdateLastAccessed();
            return entry.Value;
        }

        var value = valueFactory(key);
        var newEntry = new CacheEntry(value);

        if (_cache.Count >= _maxSize)
        {
            EvictLeastRecentlyUsed();
        }

        _cache.TryAdd(key, newEntry);
        return value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void EvictLeastRecentlyUsed()
    {
        var oldestKey = default(TKey);
        var oldestTime = DateTimeOffset.MaxValue;

        foreach (var kvp in _cache)
        {
            if (kvp.Value.LastAccessed < oldestTime)
            {
                oldestTime = kvp.Value.LastAccessed;
                oldestKey = kvp.Key;
            }
        }

        if (oldestKey is not null)
        {
            _cache.TryRemove(oldestKey, out _);
        }
    }

    private sealed class CacheEntry
    {
        public CacheEntry(TValue value)
        {
            Value = value;
            LastAccessed = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset LastAccessed { get; private set; }

        public TValue Value { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateLastAccessed() => LastAccessed = DateTimeOffset.UtcNow;
    }
}
