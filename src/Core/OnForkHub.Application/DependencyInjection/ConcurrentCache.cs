using System.Collections.Concurrent;

namespace OnForkHub.Application.DependencyInjection;

internal sealed class ConcurrentCache<TKey, TValue>
    where TKey : notnull
{
    public ConcurrentCache(int maxSize) => _maxSize = maxSize;

    private readonly ConcurrentDictionary<TKey, TValue> _cache = new();

    private readonly int _maxSize;

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (_cache.TryGetValue(key, out var value))
            return value;

        value = valueFactory(key);

        if (_cache.Count >= _maxSize)
        {
            var firstKey = _cache.Keys.FirstOrDefault();
            if (firstKey is not null)
                _cache.TryRemove(firstKey, out _);
        }

        return _cache.GetOrAdd(key, value);
    }
}
