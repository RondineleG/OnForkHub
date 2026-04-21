namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;
using System.Text.Json;

using OnForkHub.Web.Models;

/// <summary>
/// Implementation of IUserService using HttpClient.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls.</param>
    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    /// <inheritdoc/>
    public async Task<UserProfileResponse?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/api/v1/users/profile", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserProfileResponse>(_jsonOptions, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<UserProfileResponse> UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PutAsJsonAsync("/api/v1/users/profile", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserProfileResponse>(_jsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to parse update profile response");
    }

    /// <inheritdoc/>
    public async Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            "/api/v1/users/change-password",
            new { CurrentPassword = currentPassword, NewPassword = newPassword },
            cancellationToken
        );

        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public async Task DeleteAccountAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync("/api/v1/users/account", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
