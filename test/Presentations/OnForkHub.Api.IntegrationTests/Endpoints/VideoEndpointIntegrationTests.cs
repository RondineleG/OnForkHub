namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using OnForkHub.Api.IntegrationTests.Helpers;
using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Application.Dtos.User.Response;
using OnForkHub.Application.Dtos.Video.Request;

using Xunit;

/// <summary>
/// Integration tests for video endpoints.
/// Tests complete video CRUD operations with authentication and authorization.
/// </summary>
public sealed class VideoEndpointIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _authClient;
    private readonly AuthResponseDto _authResponse;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoEndpointIntegrationTests"/> class.
    /// </summary>
    public VideoEndpointIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        _authClient = _factory.CreateClient();
        _authResponse = RegisterTestUserAsync().GetAwaiter().GetResult();
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
        _authClient.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<AuthResponseDto> RegisterTestUserAsync()
    {
        var email = TestDataFactory.CreateUniqueEmail();
        var password = "Test@123456";
        var registerRequest = TestDataFactory.CreateRegisterRequest(email: email, password: password);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResponse!;
    }

    /// <summary>
    /// Tests successful video creation with valid data and authentication.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task CreateVideo_ShouldReturnCreated_WithValidData()
    {
        // Arrange
        var authClient = await AuthHelper.CreateAuthenticatedClientAsync(
            _factory,
            TestDataFactory.CreateUniqueName("VideoUser"),
            TestDataFactory.CreateUniqueEmail(),
            "Test@123456"
        );
        authClient.Should().NotBeNull();

        var request = TestDataFactory.CreateVideoRequest(userId: _authResponse.User.Id.ToString());

        // Act
        var response = await authClient!.PostAsJsonAsync("/api/v1/rest/videos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Tests that video creation fails without authentication.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task CreateVideo_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var request = TestDataFactory.CreateVideoRequest(userId: _authResponse.User.Id.ToString());

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/rest/videos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests that video creation fails with invalid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task CreateVideo_ShouldReturnBadRequest_WithInvalidData()
    {
        // Arrange
        var authClient = await AuthHelper.CreateAuthenticatedClientAsync(
            _factory,
            TestDataFactory.CreateUniqueName("VideoUser"),
            TestDataFactory.CreateUniqueEmail(),
            "Test@123456"
        );

        var request = new VideoCreateRequestDto
        {
            Title = string.Empty, // Invalid: empty title
            Description = "Valid description",
            Url = "not-a-url", // Invalid: bad URL
            UserId = _authResponse.User.Id.ToString(),
        };

        // Act
        var response = await authClient!.PostAsJsonAsync("/api/v1/rest/videos", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Tests getting all videos with pagination.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task GetAllVideos_ShouldReturnPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/rest/videos?page=1&itemsPerPage=10");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests getting video by ID when it exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task GetVideoById_ShouldReturnVideo_WhenExists()
    {
        // Arrange - Create a video first
        var authClient = await AuthHelper.CreateAuthenticatedClientAsync(
            _factory,
            TestDataFactory.CreateUniqueName("VideoUser"),
            TestDataFactory.CreateUniqueEmail(),
            "Test@123456"
        );

        var createRequest = TestDataFactory.CreateVideoRequest(userId: _authResponse.User.Id.ToString());
        var createResponse = await authClient!.PostAsJsonAsync("/api/v1/rest/videos", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Extract video ID from location header or response
        var locationHeader = createResponse.Headers.Location?.ToString();
        locationHeader.Should().NotBeNullOrEmpty();

        var videoId = locationHeader?.Split('/').LastOrDefault();
        videoId.Should().NotBeNullOrEmpty();

        // Act
        var response = await _client.GetAsync($"/api/v1/rest/videos/{videoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests getting video by ID when it doesn't exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task GetVideoById_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = "99999999";

        // Act
        var response = await _client.GetAsync($"/api/v1/rest/videos/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests updating a video with valid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task UpdateVideo_ShouldReturnUpdatedVideo_WithValidData()
    {
        // Arrange - Create a video first
        var authClient = await AuthHelper.CreateAuthenticatedClientAsync(
            _factory,
            TestDataFactory.CreateUniqueName("VideoUser"),
            TestDataFactory.CreateUniqueEmail(),
            "Test@123456"
        );

        var createRequest = TestDataFactory.CreateVideoRequest(userId: _authResponse.User.Id.ToString());
        var createResponse = await authClient!.PostAsJsonAsync("/api/v1/rest/videos", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = createResponse.Headers.Location?.ToString();
        var videoId = locationHeader?.Split('/').LastOrDefault();
        videoId.Should().NotBeNullOrEmpty();

        var updateRequest = TestDataFactory.CreateVideoUpdateRequest(videoId!, "Updated Video Title");

        // Act
        var response = await authClient!.PutAsJsonAsync($"/api/v1/rest/videos/{videoId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that updating a video fails without authentication.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task UpdateVideo_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var updateRequest = TestDataFactory.CreateVideoUpdateRequest(videoId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/rest/videos/{videoId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests deleting a video.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task DeleteVideo_ShouldReturnNoContent_WithValidData()
    {
        // Arrange - Create a video first
        var authClient = await AuthHelper.CreateAuthenticatedClientAsync(
            _factory,
            TestDataFactory.CreateUniqueName("VideoUser"),
            TestDataFactory.CreateUniqueEmail(),
            "Test@123456"
        );

        var createRequest = TestDataFactory.CreateVideoRequest(userId: _authResponse.User.Id.ToString());
        var createResponse = await authClient!.PostAsJsonAsync("/api/v1/rest/videos", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = createResponse.Headers.Location?.ToString();
        var videoId = locationHeader?.Split('/').LastOrDefault();
        videoId.Should().NotBeNullOrEmpty();

        // Act
        var response = await authClient!.DeleteAsync($"/api/v1/rest/videos/{videoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that deleting a video fails without authentication.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task DeleteVideo_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/rest/videos/{videoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests that deleting a non-existent video returns not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Video")]
    public async Task DeleteVideo_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var authClient = await AuthHelper.CreateAuthenticatedClientAsync(
            _factory,
            TestDataFactory.CreateUniqueName("VideoUser"),
            TestDataFactory.CreateUniqueEmail(),
            "Test@123456"
        );
        var nonExistentId = "99999999";

        // Act
        var response = await authClient!.DeleteAsync($"/api/v1/rest/videos/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
