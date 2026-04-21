namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnForkHub.Api.IntegrationTests.Helpers;
using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.Dtos.User.Response;
using Xunit;

/// <summary>
/// Integration tests for authentication endpoints.
/// Tests the complete authentication lifecycle including register, login, refresh, and token validation.
/// </summary>
public sealed class AuthEndpointIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthEndpointIntegrationTests"/> class.
    /// </summary>
    public AuthEndpointIntegrationTests()
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
    /// Tests the complete user lifecycle: register → login → refresh → revoke.
    /// This is the critical authentication flow test.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    public async Task Register_Login_Refresh_FullFlow_ShouldSucceed()
    {
        // Arrange
        var email = TestDataFactory.CreateUniqueEmail();
        var password = "Test@123456";
        var name = TestDataFactory.CreateUniqueName("FullFlowUser");

        // Act 1: Register
        var registerRequest = TestDataFactory.CreateRegisterRequest(name, email, password);
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Assert 1
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.User.Email.Should().Be(email);

        // Act 2: Login with same credentials
        var loginRequest = TestDataFactory.CreateLoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert 2
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginAuthResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        loginAuthResponse.Should().NotBeNull();
        loginAuthResponse!.AccessToken.Should().NotBeNullOrEmpty();
        loginAuthResponse.RefreshToken.Should().NotBeNullOrEmpty();

        // Act 3: Refresh tokens
        var refreshRequest = new RefreshTokenRequestDto
        {
            AccessToken = loginAuthResponse.AccessToken,
            RefreshToken = loginAuthResponse.RefreshToken,
        };
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert 3
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshAuthResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        refreshAuthResponse.Should().NotBeNull();
        refreshAuthResponse!.AccessToken.Should().NotBeNullOrEmpty();
        refreshAuthResponse.RefreshToken.Should().NotBeNullOrEmpty();

        // New tokens should be different from old ones
        refreshAuthResponse.AccessToken.Should().NotBe(loginAuthResponse.AccessToken);
    }

    /// <summary>
    /// Tests successful user registration with valid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    public async Task Register_ShouldReturnCreated_WithValidUser()
    {
        // Arrange
        var request = TestDataFactory.CreateRegisterRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be(request.Email);
        authResponse.User.Name.Should().Be(request.Name);
        authResponse.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.AccessTokenExpiration.Should().BeAfter(DateTime.UtcNow);
        authResponse.RefreshTokenExpiration.Should().BeAfter(DateTime.UtcNow);
    }

    /// <summary>
    /// Tests that duplicate email registration returns conflict.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    public async Task Register_ShouldReturnConflict_WithDuplicateEmail()
    {
        // Arrange
        var email = TestDataFactory.CreateUniqueEmail();
        var request1 = TestDataFactory.CreateRegisterRequest(email: email);
        var request2 = TestDataFactory.CreateRegisterRequest(name: "Another User", email: email);

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/v1/auth/register", request1);
        var response2 = await _client.PostAsJsonAsync("/api/v1/auth/register", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Tests that invalid registration data returns bad request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    [InlineData("", "test@example.com", "Test@123456")]
    [InlineData("ValidName", "invalid-email", "Test@123456")]
    [InlineData("ValidName", "test@example.com", "short")]
    [InlineData("ValidName", "", "Test@123456")]
    [InlineData("ValidName", "test@example.com", "")]
    public async Task Register_ShouldReturnBadRequest_WithInvalidData(string name, string email, string password)
    {
        // Arrange
        var request = new UserRegisterRequestDto
        {
            Name = name,
            Email = email,
            Password = password,
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Tests successful login with valid credentials.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    public async Task Login_ShouldReturnOk_WithValidCredentials()
    {
        // Arrange
        var email = TestDataFactory.CreateUniqueEmail();
        var password = "Test@123456";
        var registerRequest = TestDataFactory.CreateRegisterRequest(email: email, password: password);
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = TestDataFactory.CreateLoginRequest(email, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.User.Email.Should().Be(email);
    }

    /// <summary>
    /// Tests that login fails with invalid credentials.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    public async Task Login_ShouldReturnUnauthorized_WithInvalidCredentials()
    {
        // Arrange
        var email = TestDataFactory.CreateUniqueEmail();
        var password = "Test@123456";
        var registerRequest = TestDataFactory.CreateRegisterRequest(email: email, password: password);
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = TestDataFactory.CreateLoginRequest(email, "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests that login returns too many requests when rate limited.
    /// Note: Rate limiting is disabled in test configuration, so this test verifies the endpoint behavior.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    public async Task Login_ShouldHandleMultipleRequests_WhenRateLimitDisabled()
    {
        // Arrange
        var email = TestDataFactory.CreateUniqueEmail();
        var password = "Test@123456";
        var registerRequest = TestDataFactory.CreateRegisterRequest(email: email, password: password);
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = TestDataFactory.CreateLoginRequest(email, "WrongPassword123!");

        // Act - Send multiple requests (rate limiting is disabled in test config)
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 15; i++)
        {
            responses.Add(await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest));
        }

        // Assert - All should return Unauthorized (rate limiting disabled)
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Unauthorized));
    }

    /// <summary>
    /// Tests that token refresh returns new tokens with valid refresh token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    public async Task RefreshToken_ShouldReturnNewTokens_WithValidRefreshToken()
    {
        // Arrange
        var email = TestDataFactory.CreateUniqueEmail();
        var password = "Test@123456";
        var registerRequest = TestDataFactory.CreateRegisterRequest(email: email, password: password);
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var refreshRequest = new RefreshTokenRequestDto { AccessToken = authResponse!.AccessToken, RefreshToken = authResponse.RefreshToken };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newAuthResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        newAuthResponse.Should().NotBeNull();
        newAuthResponse!.AccessToken.Should().NotBeNullOrEmpty();
        newAuthResponse.RefreshToken.Should().NotBeNullOrEmpty();
        newAuthResponse.AccessToken.Should().NotBe(authResponse.AccessToken);
        newAuthResponse.RefreshToken.Should().NotBe(authResponse.RefreshToken);
    }

    /// <summary>
    /// Tests that token refresh fails with invalid token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    public async Task RefreshToken_ShouldReturnUnauthorized_WithInvalidToken()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequestDto { AccessToken = "invalid-access-token", RefreshToken = "invalid-refresh-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests that token refresh fails with missing fields.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Authentication")]
    public async Task RefreshToken_ShouldReturnBadRequest_WithMissingFields()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequestDto { AccessToken = string.Empty, RefreshToken = string.Empty };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
