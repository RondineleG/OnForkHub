namespace OnForkHub.Api.IntegrationTests.Endpoints;

using OnForkHub.Api.IntegrationTests.Infrastructure;

[TestClass]
[TestCategory("Integration")]
public sealed class CategoriesEndpointTests
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
    public async Task GetCategoriesEndpointShouldReturnMethodNotAllowed()
    {
        // The API only supports POST for categories, not GET
        var response = await _client.GetAsync("/api/v1/rest/categories");

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [TestMethod]
    public async Task CreateCategoryWithInvalidDataShouldReturnBadRequest()
    {
        var invalidCategory = new { Name = string.Empty };

        var response = await _client.PostAsJsonAsync("/api/v1/rest/categories", invalidCategory);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [TestMethod]
    public async Task CreateCategoryWithValidDataShouldReturnCreated()
    {
        var validCategory = new { Name = $"Test Category {Guid.NewGuid():N}", Description = "Integration test category" };

        var response = await _client.PostAsJsonAsync("/api/v1/rest/categories", validCategory);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [TestMethod]
    public async Task CreateCategoryResponseShouldBeJson()
    {
        var validCategory = new { Name = $"Test Category {Guid.NewGuid():N}", Description = "Integration test category" };

        var response = await _client.PostAsJsonAsync("/api/v1/rest/categories", validCategory);

        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
    }

    [TestMethod]
    public async Task NonExistentEndpointShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/v1/rest/nonexistent");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
