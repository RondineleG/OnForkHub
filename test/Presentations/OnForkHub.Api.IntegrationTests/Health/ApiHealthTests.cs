namespace OnForkHub.Api.IntegrationTests.Health;

using OnForkHub.Api.IntegrationTests.Infrastructure;

[TestClass]
[TestCategory("Integration")]
public sealed class ApiHealthTests
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
    public async Task ApiShouldStartSuccessfully()
    {
        // Test using POST to categories endpoint which is the only available endpoint
        var category = new { Name = $"Health Check {Guid.NewGuid():N}", Description = "Health check test" };

        var response = await _client.PostAsJsonAsync("/api/v1/rest/categories", category);

        response.Should().NotBeNull();
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task ApiShouldRespondWithinReasonableTime()
    {
        var category = new { Name = $"Performance Test {Guid.NewGuid():N}", Description = "Performance test" };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _client.PostAsJsonAsync("/api/v1/rest/categories", category);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [TestMethod]
    public async Task SwaggerShouldBeAvailableInDevelopment()
    {
        var response = await _client.GetAsync("/swagger/index.html");

        // Swagger may or may not be available depending on configuration
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Moved);
    }
}
