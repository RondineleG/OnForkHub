namespace OnForkHub.Web.Services.Api;

using OnForkHub.Web.Models;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

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
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc/>
    public async Task<UserProfileDto?> GetProfileAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/users/profile");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>(_jsonOptions);
        return result;
    }

    /// <inheritdoc/>
    public async Task<UserProfileDto> UpdateProfileAsync(string name, string? avatarUrl)
    {
        var response = await _httpClient.PutAsJsonAsync("/api/v1/users/profile", new
        {
            Name = name,
            AvatarUrl = avatarUrl
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to parse update profile response");
    }

    /// <inheritdoc/>
    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var response = await _httpClient.PutAsJsonAsync("/api/v1/users/change-password", new
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        });

        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public async Task DeleteAccountAsync()
    {
        var response = await _httpClient.DeleteAsync("/api/v1/users/account");
        response.EnsureSuccessStatusCode();
    }
}
