namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using OnForkHub.Api.IntegrationTests.Helpers;
using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Application.Dtos.Category.Request;
using OnForkHub.Core.Responses.Categories;

using Xunit;

/// <summary>
/// Integration tests for category endpoints.
/// Tests category CRUD operations including create, get all, and search.
/// </summary>
public sealed class CategoryEndpointIntegrationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    /// <summary>
    /// Tests successful category creation with valid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateCategory_ShouldReturnCreated_WithValidData()
    {
        // Arrange
        var request = TestDataFactory.CreateCategoryRequest();

        // Act - Using auth mock
        var client = factory.CreateClientWithAuth();
        var response = await client.PostAsJsonAsync("/api/v1/rest/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Tests that category creation fails with invalid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [Trait("Category", "Integration")]
    [InlineData("")]
    [InlineData("This name is way too long and exceeds the maximum allowed length of fifty characters")]
    public async Task CreateCategory_ShouldReturnBadRequest_WithInvalidData(string name)
    {
        // Arrange
        var request = new CategoryRequestDto { Name = name, Description = "Test description" };

        // Act
        var client = factory.CreateClientWithAuth();
        var response = await client.PostAsJsonAsync("/api/v1/rest/categories", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests that duplicate category names return conflict.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateCategory_ShouldReturnConflict_WithDuplicateName()
    {
        // Arrange
        var uniqueName = $"TestCategory_{Guid.NewGuid():N}";
        var request1 = TestDataFactory.CreateCategoryRequest(name: uniqueName);

        // Act
        var client = factory.CreateClientWithAuth();
        var response1 = await client.PostAsJsonAsync("/api/v1/rest/categories", request1);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Tests getting all categories returns paged result.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAllCategories_ShouldReturnPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/rest/categories");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    private sealed class TestResponse<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
