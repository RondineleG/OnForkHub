namespace OnForkHub.Api.IntegrationTests.Helpers;

using System.Net.Http.Json;

using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.Dtos.User.Response;

/// <summary>
/// Helper class for authentication operations in integration tests.
/// </summary>
public static class AuthHelper
{
    /// <summary>
    /// Registers a new user and returns the auth response with tokens.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="name">User name.</param>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns>Auth response with tokens, or null if registration failed.</returns>
    public static async Task<AuthResponseDto?> RegisterUserAsync(HttpClient client, string name, string email, string password)
    {
        var registerRequest = new UserRegisterRequestDto
        {
            Name = name,
            Email = email,
            Password = password,
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    }

    /// <summary>
    /// Logs in a user and returns the auth response with tokens.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns>Auth response with tokens, or null if login failed.</returns>
    public static async Task<AuthResponseDto?> LoginUserAsync(HttpClient client, string email, string password)
    {
        var loginRequest = new UserLoginRequestDto { Email = email, Password = password };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    }

    /// <summary>
    /// Registers a user and returns an authenticated HTTP client.
    /// </summary>
    /// <param name="factory">Web application factory.</param>
    /// <param name="name">User name.</param>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns>Authenticated HttpClient, or null if registration failed.</returns>
    public static async Task<HttpClient?> CreateAuthenticatedClientAsync(
        CustomWebApplicationFactory factory,
        string name,
        string email,
        string password
    )
    {
        var client = factory.CreateClient();

        var authResponse = await RegisterUserAsync(client, name, email, password);

        if (authResponse == null)
        {
            return null;
        }

        return factory.CreateClientWithAuth(authResponse.AccessToken);
    }

    /// <summary>
    /// Refreshes tokens using the refresh token endpoint.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="accessToken">Current access token.</param>
    /// <param name="refreshToken">Current refresh token.</param>
    /// <returns>New auth response with tokens, or null if refresh failed.</returns>
    public static async Task<AuthResponseDto?> RefreshTokensAsync(HttpClient client, string accessToken, string refreshToken)
    {
        var refreshRequest = new RefreshTokenRequestDto { AccessToken = accessToken, RefreshToken = refreshToken };

        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    }
}
