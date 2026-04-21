namespace OnForkHub.Web.Auth;

using Microsoft.AspNetCore.Components.Authorization;

using OnForkHub.Web.Services;

using System.Globalization;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

/// <summary>
/// Provides authentication state management for Blazor WebAssembly.
/// Stores JWT tokens in localStorage and exposes user claims.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "authToken";
    private const string RefreshTokenKey = "refreshToken";

    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtAuthenticationStateProvider"/> class.
    /// </summary>
    /// <param name="localStorage">Local storage service for token persistence.</param>
    /// <param name="httpClient">HTTP client for API calls.</param>
    public JwtAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <inheritdoc/>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var savedToken = await _localStorage.GetItemAsync<string>(AccessTokenKey);

        if (string.IsNullOrWhiteSpace(savedToken))
        {
            return new AuthenticationState(_anonymous);
        }

        var claims = ParseClaimsFromJwt(savedToken);
        var expiration = GetTokenExpiration(claims);

        if (expiration <= DateTime.UtcNow)
        {
            var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var refreshed = await TryRefreshTokenAsync(refreshToken, savedToken);
                if (refreshed)
                {
                    return await GetAuthenticationStateAsync();
                }
            }

            await ClearAuth();
            return new AuthenticationState(_anonymous);
        }

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        return new AuthenticationState(authenticatedUser);
    }

    /// <summary>
    /// Marks the user as authenticated by storing tokens and notifying the application.
    /// </summary>
    /// <param name="token">The JWT access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task MarkUserAsAuthenticated(string token, string refreshToken)
    {
        await _localStorage.SetItemAsync(AccessTokenKey, token);
        await _localStorage.SetItemAsync(RefreshTokenKey, refreshToken);

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var claims = ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));
    }

    /// <summary>
    /// Logs out the user by clearing stored tokens.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task Logout()
    {
        await ClearAuth();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = DecodeBase64Url(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null)
            {
                return Enumerable.Empty<Claim>();
            }

            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
        }
        catch
        {
            return Enumerable.Empty<Claim>();
        }
    }

    private static DateTime GetTokenExpiration(IEnumerable<Claim> claims)
    {
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim == null)
        {
            return DateTime.MinValue;
        }

        var exp = double.Parse(expClaim.Value, CultureInfo.InvariantCulture);
        return DateTimeOffset.FromUnixTimeSeconds((long)exp).UtcDateTime;
    }

    private static byte[] DecodeBase64Url(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }

    private async Task<bool> TryRefreshTokenAsync(string refreshToken, string expiredAccessToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/v1/auth/refresh",
                new { AccessToken = expiredAccessToken, RefreshToken = refreshToken }
            );

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokens = JsonSerializer.Deserialize<TokenResponse>(content, JsonOptions);

            if (tokens == null)
            {
                return false;
            }

            await _localStorage.SetItemAsync(AccessTokenKey, tokens.AccessToken);
            await _localStorage.SetItemAsync(RefreshTokenKey, tokens.RefreshToken);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task ClearAuth()
    {
        await _localStorage.RemoveItemAsync(AccessTokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private sealed class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
