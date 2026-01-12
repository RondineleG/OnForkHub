namespace OnForkHub.CrossCutting.Tests.Caching;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OnForkHub.CrossCutting.Caching;
using OnForkHub.CrossCutting.Caching.Implementations;

[TestClass]
[TestCategory("Unit")]
public sealed class MemoryCacheServiceTests : IDisposable
{
    private MemoryCacheService _cacheService = null!;
    private MemoryCache? _memoryCache;
    private bool _disposed;

    [TestInitialize]
    public void Setup()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new CacheOptions { DefaultExpirationMinutes = 30, InstanceName = "Test_" });
        _cacheService = new MemoryCacheService(_memoryCache, options);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _memoryCache?.Dispose();
            _disposed = true;
        }
    }

    [TestMethod]
    [TestCategory("Caching")]
    public async Task GetAsyncReturnsNullWhenKeyNotFound()
    {
        var result = await _cacheService.GetAsync<TestCacheItem>("nonexistent");

        Assert.IsNull(result);
    }

    [TestMethod]
    [TestCategory("Caching")]
    public async Task SetAsyncAndGetAsyncWorkCorrectly()
    {
        var item = new TestCacheItem { Id = 1, Name = "Test" };

        await _cacheService.SetAsync("test-key", item);
        var result = await _cacheService.GetAsync<TestCacheItem>("test-key");

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Test", result.Name);
    }

    [TestMethod]
    [TestCategory("Caching")]
    public async Task SetAsyncWithExpirationStoresItem()
    {
        var item = new TestCacheItem { Id = 2, Name = "Expiring" };

        await _cacheService.SetAsync("expiring-key", item, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<TestCacheItem>("expiring-key");

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Id);
    }

    [TestMethod]
    [TestCategory("Caching")]
    public async Task RemoveAsyncRemovesItem()
    {
        var item = new TestCacheItem { Id = 3, Name = "ToRemove" };
        await _cacheService.SetAsync("remove-key", item);

        await _cacheService.RemoveAsync("remove-key");
        var result = await _cacheService.GetAsync<TestCacheItem>("remove-key");

        Assert.IsNull(result);
    }

    [TestMethod]
    [TestCategory("Caching")]
    public async Task ExistsAsyncReturnsTrueWhenKeyExists()
    {
        var item = new TestCacheItem { Id = 4, Name = "Exists" };
        await _cacheService.SetAsync("exists-key", item);

        var exists = await _cacheService.ExistsAsync("exists-key");

        Assert.IsTrue(exists);
    }

    [TestMethod]
    [TestCategory("Caching")]
    public async Task ExistsAsyncReturnsFalseWhenKeyNotExists()
    {
        var exists = await _cacheService.ExistsAsync("not-exists-key");

        Assert.IsFalse(exists);
    }

    [TestMethod]
    [TestCategory("Caching")]
    public async Task GetOrCreateAsyncReturnsCachedItemWhenExists()
    {
        var item = new TestCacheItem { Id = 5, Name = "Cached" };
        await _cacheService.SetAsync("getorcreate-key", item);
        var factoryCalled = false;

        var result = await _cacheService.GetOrCreateAsync(
            "getorcreate-key",
            async _ =>
            {
                factoryCalled = true;
                return await Task.FromResult(new TestCacheItem { Id = 99, Name = "New" });
            },
            TimeSpan.FromMinutes(5)
        );

        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Id);
        Assert.IsFalse(factoryCalled);
    }

    [TestMethod]
    [TestCategory("Caching")]
    public async Task GetOrCreateAsyncCallsFactoryWhenNotCached()
    {
        var factoryCalled = false;

        var result = await _cacheService.GetOrCreateAsync(
            "new-getorcreate-key",
            async _ =>
            {
                factoryCalled = true;
                return await Task.FromResult(new TestCacheItem { Id = 10, Name = "Created" });
            },
            TimeSpan.FromMinutes(5)
        );

        Assert.IsNotNull(result);
        Assert.AreEqual(10, result.Id);
        Assert.IsTrue(factoryCalled);
    }

    [TestMethod]
    [TestCategory("Caching")]
    public async Task RemoveByPatternAsyncRemovesMatchingKeys()
    {
        await _cacheService.SetAsync("pattern:item1", new TestCacheItem { Id = 1, Name = "Item1" });
        await _cacheService.SetAsync("pattern:item2", new TestCacheItem { Id = 2, Name = "Item2" });
        await _cacheService.SetAsync("other:item", new TestCacheItem { Id = 3, Name = "Other" });

        await _cacheService.RemoveByPatternAsync("pattern:");

        Assert.IsFalse(await _cacheService.ExistsAsync("pattern:item1"));
        Assert.IsFalse(await _cacheService.ExistsAsync("pattern:item2"));
        Assert.IsTrue(await _cacheService.ExistsAsync("other:item"));
    }

    [TestMethod]
    [TestCategory("Caching")]
    public void ConstructorThrowsWhenCacheIsNull()
    {
        var options = Options.Create(new CacheOptions());

        Assert.ThrowsExactly<ArgumentNullException>(() => new MemoryCacheService(null!, options));
    }

    [TestMethod]
    [TestCategory("Caching")]
    public void ConstructorThrowsWhenOptionsIsNull()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());

        Assert.ThrowsExactly<ArgumentNullException>(() => new MemoryCacheService(cache, null!));
    }

    private sealed class TestCacheItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
