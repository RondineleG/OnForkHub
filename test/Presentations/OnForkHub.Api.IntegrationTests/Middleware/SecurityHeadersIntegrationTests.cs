namespace OnForkHub.Api.IntegrationTests.Middleware;

using OnForkHub.Api.IntegrationTests.Infrastructure;

[TestClass]
[TestCategory("Integration")]
public sealed class SecurityHeadersIntegrationTests
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
    public async Task ResponseShouldContainXContentTypeOptionsHeader()
    {
        var response = await _client.GetAsync("/api/v1/rest/categories");

        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
    }

    [TestMethod]
    public async Task ResponseShouldContainXFrameOptionsHeader()
    {
        var response = await _client.GetAsync("/api/v1/rest/categories");

        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");
    }

    [TestMethod]
    public async Task ResponseShouldContainXXssProtectionHeader()
    {
        var response = await _client.GetAsync("/api/v1/rest/categories");

        response.Headers.Should().ContainKey("X-XSS-Protection");
    }

    [TestMethod]
    public async Task ResponseShouldContainReferrerPolicyHeader()
    {
        var response = await _client.GetAsync("/api/v1/rest/categories");

        response.Headers.Should().ContainKey("Referrer-Policy");
    }

    [TestMethod]
    public async Task ResponseShouldContainContentSecurityPolicyHeader()
    {
        var response = await _client.GetAsync("/api/v1/rest/categories");

        response.Headers.Should().ContainKey("Content-Security-Policy");
    }
}
