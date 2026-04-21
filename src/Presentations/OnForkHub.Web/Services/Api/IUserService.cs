namespace OnForkHub.Web.Services.Api;

using OnForkHub.Web.Models;

/// <summary>
/// Service contract for user profile API operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<UserProfileDto?> GetProfileAsync();

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<UserProfileDto> UpdateProfileAsync(string name, string? avatarUrl);

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task ChangePasswordAsync(string currentPassword, string newPassword);

    /// <summary>
    /// Deletes the current user's account.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAccountAsync();
}
