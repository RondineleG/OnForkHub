namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;
using System.Threading.Tasks;

/// <summary>
/// Implementation of IAuthService using HttpClient.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls.</param>
    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result ?? throw new InvalidOperationException("Failed to parse login response");
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> RegisterAsync(string name, string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Name = name,
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result ?? throw new InvalidOperationException("Failed to parse register response");
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result ?? throw new InvalidOperationException("Failed to parse refresh token response");
    }

    /// <inheritdoc/>
    public Task LogoutAsync()
    {
        // Client-side logout - tokens will be cleared by the auth provider
        return Task.CompletedTask;
    }
}
