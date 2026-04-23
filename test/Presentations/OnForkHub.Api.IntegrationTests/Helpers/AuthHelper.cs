namespace OnForkHub.Api.IntegrationTests.Helpers;

using System.Net.Http.Json;

using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Core.Responses.Users;

public static class AuthHelper
{
    public static async Task<AuthResponse?> RegisterNewUserAsync(
        HttpClient client,
        string? name = null,
        string? email = null,
        string? password = null
    )
    {
        var request = TestDataFactory.CreateRegisterRequest(name, email, password ?? string.Empty);
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public static async Task<AuthResponse?> LoginAsync(HttpClient client, string email, string password)
    {
        var request = TestDataFactory.CreateLoginRequest(email, password);
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public static async Task<AuthResponse?> GetAuthenticatedClientAsync(
        CustomWebApplicationFactory factory,
        string? email = null,
        string? password = null
    )
    {
        var client = factory.CreateClient();
        var authResponse = await RegisterNewUserAsync(client, email: email, password: password);

        return authResponse;
    }

    public static HttpClient CreateAuthenticatedClient(CustomWebApplicationFactory factory, AuthResponse authResponse)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        return client;
    }

    public static async Task<AuthResponse?> RefreshTokenAsync(HttpClient client, string accessToken, string refreshToken)
    {
        var refreshRequest = new RefreshTokenRequestDto { AccessToken = accessToken, RefreshToken = refreshToken };

        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }
}
