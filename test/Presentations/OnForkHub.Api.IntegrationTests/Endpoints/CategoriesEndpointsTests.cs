namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;

using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Core.Responses.Categories;

using Xunit;

public class CategoriesEndpointsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCategories_ReturnsOkOrNoContent()
    {
        var response = await _client.GetAsync("/api/v1/rest/categories");
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateCategory_ReturnsCreated_WhenValid()
    {
        var request = new { Name = "Test Category", Description = "Test Description" };
        var client = factory.CreateClientWithAuth();
        var response = await client.PostAsJsonAsync("/api/v1/rest/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCategoryById_ReturnsNoContent_WhenNotFound()
    {
        var response = await _client.GetAsync("/api/v1/rest/categories/999999");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpdateCategory_ReturnsBadRequest_WhenNotFound()
    {
        var request = new { Name = "Updated Category", Description = "Updated Description" };
        var client = factory.CreateClientWithAuth();
        var response = await client.PutAsJsonAsync("/api/v1/rest/categories/999999", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteCategory_ReturnsBadRequest_WhenNotFound()
    {
        var client = factory.CreateClientWithAuth();
        var response = await client.DeleteAsync("/api/v1/rest/categories/999999");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchCategories_ReturnsOkOrNoContent()
    {
        var response = await _client.GetAsync("/api/v1/rest/categories/search?searchTerm=test&page=1&itemsPerPage=10");
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent);
    }
}
