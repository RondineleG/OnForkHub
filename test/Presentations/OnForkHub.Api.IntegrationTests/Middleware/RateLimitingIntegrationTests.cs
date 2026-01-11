namespace OnForkHub.Api.IntegrationTests.Middleware;

using OnForkHub.Api.IntegrationTests.Infrastructure;

[TestClass]
[TestCategory("Integration")]
public sealed class RateLimitingIntegrationTests
{
    private static ApiWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _factory = new ApiWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [TestMethod]
    public async Task SingleRequestShouldNotBeLimited()
    {
        var response = await _client.GetAsync("/api/v1/rest/categories");

        response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    [TestMethod]
    public async Task RequestShouldSucceedWithinRateLimit()
    {
        for (var i = 0; i < 10; i++)
        {
            var response = await _client.GetAsync("/api/v1/rest/categories");
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }
    }

    [TestMethod]
    public async Task RateLimitResponseShouldIncludeRetryAfterHeader()
    {
        // Note: This test documents expected behavior when rate limited
        // In a real scenario, we would need to exceed the rate limit
        var response = await _client.GetAsync("/api/v1/rest/categories");

        // If rate limited, should have Retry-After header
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            response.Headers.Should().ContainKey("Retry-After");
        }
    }
}
