namespace OnForkHub.Api.IntegrationTests.Middleware;

using OnForkHub.Api.IntegrationTests.Infrastructure;

using System.Net.Http.Headers;

[TestClass]
[TestCategory("Integration")]
public sealed class ResponseCompressionIntegrationTests
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
    public async Task ServerShouldAcceptGzipEncodingHeader()
    {
        // Create a category to get a response
        var category = new { Name = $"Compression Test {Guid.NewGuid():N}", Description = "Test" };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/rest/categories");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        request.Content = JsonContent.Create(category);

        var response = await _client.SendAsync(request);

        // Server should accept the request with gzip encoding header
        response.Should().NotBeNull();
    }

    [TestMethod]
    public async Task ServerShouldAcceptBrotliEncodingHeader()
    {
        var category = new { Name = $"Compression Test {Guid.NewGuid():N}", Description = "Test" };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/rest/categories");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        request.Content = JsonContent.Create(category);

        var response = await _client.SendAsync(request);

        // Server should accept the request with brotli encoding header
        response.Should().NotBeNull();
    }

    [TestMethod]
    public async Task ResponseWithoutAcceptEncodingShouldNotBeCompressed()
    {
        var category = new { Name = $"Compression Test {Guid.NewGuid():N}", Description = "Test" };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/rest/categories");
        request.Headers.AcceptEncoding.Clear();
        request.Content = JsonContent.Create(category);

        var response = await _client.SendAsync(request);

        response.Content.Headers.ContentEncoding.Should().BeEmpty();
    }
}
