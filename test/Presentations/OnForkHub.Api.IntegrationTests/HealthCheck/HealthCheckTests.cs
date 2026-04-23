namespace OnForkHub.Api.IntegrationTests.HealthCheck;

using System.Net;

using OnForkHub.Api.IntegrationTests.Infrastructure;

using Xunit;

public class HealthCheckTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    [Trait("Category", "Integration")]
    public async Task HealthCheck_ReturnsOk()
    {
        // Using categories endpoint as a simple health check since it's publicly accessible
        var response = await _client.GetAsync("/api/v1/rest/categories");

        // We expect OK (200) or No Content (204) if database is empty
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent);
    }
}
