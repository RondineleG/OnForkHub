namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnForkHub.Api.IntegrationTests.Helpers;
using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Application.Dtos.Category.Request;
using Xunit;

/// <summary>
/// Integration tests for category endpoints.
/// Tests category CRUD operations including create, get all, and search.
/// </summary>
public sealed class CategoryEndpointIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryEndpointIntegrationTests"/> class.
    /// </summary>
    public CategoryEndpointIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    /// <summary>
    /// Tests successful category creation with valid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    public async Task CreateCategory_ShouldReturnCreated_WithValidData()
    {
        // Arrange
        var request = TestDataFactory.CreateCategoryRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/rest/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Tests that category creation fails with invalid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    [InlineData("")]
    [InlineData("This name is way too long and exceeds the maximum allowed length of fifty characters")]
    public async Task CreateCategory_ShouldReturnBadRequest_WithInvalidData(string name)
    {
        // Arrange
        var request = new CategoryRequestDto { Name = name, Description = "Test description" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/rest/categories", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Tests that duplicate category names return conflict.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    public async Task CreateCategory_ShouldReturnConflict_WithDuplicateName()
    {
        // Arrange
        var uniqueName = $"TestCategory_{Guid.NewGuid():N}";
        var request1 = TestDataFactory.CreateCategoryRequest(name: uniqueName);
        var request2 = TestDataFactory.CreateCategoryRequest(name: uniqueName, description: "Different description");

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/v1/rest/categories", request1);
        var response2 = await _client.PostAsJsonAsync("/api/v1/rest/categories", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Tests getting all categories returns paged result.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    public async Task GetAllCategories_ShouldReturnPagedResult()
    {
        // Arrange - Create some categories first
        var category1 = TestDataFactory.CreateCategoryRequest(name: "Category1");
        var category2 = TestDataFactory.CreateCategoryRequest(name: "Category2");
        await _client.PostAsJsonAsync("/api/v1/rest/categories", category1);
        await _client.PostAsJsonAsync("/api/v1/rest/categories", category2);

        // Act
        var response = await _client.GetAsync("/api/v1/rest/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests searching categories with search term.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    public async Task SearchCategories_ShouldReturnFilteredResults()
    {
        // Arrange - Create a category with unique name
        var uniqueName = $"SearchableCategory_{Guid.NewGuid():N}";
        var request = TestDataFactory.CreateCategoryRequest(name: uniqueName);
        await _client.PostAsJsonAsync("/api/v1/rest/categories", request);

        // Act
        var response = await _client.GetAsync($"/api/v1/rest/categories/search?searchTerm={uniqueName}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests searching categories with pagination.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    public async Task SearchCategories_ShouldSupportPagination()
    {
        // Arrange - Create multiple categories
        for (int i = 0; i < 5; i++)
        {
            var request = TestDataFactory.CreateCategoryRequest(name: $"PageCategory_{i}_{Guid.NewGuid():N}");
            await _client.PostAsJsonAsync("/api/v1/rest/categories", request);
        }

        // Act
        var response = await _client.GetAsync("/api/v1/rest/categories/search?page=1&itemsPerPage=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests searching categories with sorting.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    public async Task SearchCategories_ShouldSupportSorting()
    {
        // Arrange - Create multiple categories
        for (int i = 0; i < 3; i++)
        {
            var request = TestDataFactory.CreateCategoryRequest(name: $"SortCategory_{i}");
            await _client.PostAsJsonAsync("/api/v1/rest/categories", request);
        }

        // Act
        var responseAsc = await _client.GetAsync("/api/v1/rest/categories/search?sortDescending=false");
        var responseDesc = await _client.GetAsync("/api/v1/rest/categories/search?sortDescending=true");

        // Assert
        responseAsc.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        responseDesc.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that creating a category with missing required fields fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenNameIsMissing()
    {
        // Arrange
        var request = new CategoryRequestDto { Name = string.Empty, Description = "Test description" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/rest/categories", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Tests that categories can be retrieved without authentication.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    public async Task GetAllCategories_ShouldWorkWithoutAuthentication()
    {
        // Arrange - Create a category
        var request = TestDataFactory.CreateCategoryRequest();
        await _client.PostAsJsonAsync("/api/v1/rest/categories", request);

        // Act
        var response = await _client.GetAsync("/api/v1/rest/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that category creation response contains valid JSON.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Category")]
    public async Task CreateCategory_ResponseShouldBeValidJson()
    {
        // Arrange
        var request = TestDataFactory.CreateCategoryRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/rest/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
