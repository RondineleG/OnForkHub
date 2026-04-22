namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;
using System.Text.Json;

using OnForkHub.Core.Requests.Users;
using OnForkHub.Core.Responses.Users;

/// <summary>
/// Implementation of IUserService using HttpClient.
/// </summary>
public sealed class UserService(HttpClient httpClient) : IUserService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <inheritdoc/>
    public async Task<UserProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/api/v1/users/profile", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserProfileResponse>(_jsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to load user profile");
    }

    /// <inheritdoc/>
    public async Task<UserProfileResponse> UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync("/api/v1/users/profile", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserProfileResponse>(_jsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to update user profile");
    }
}
