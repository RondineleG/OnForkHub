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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<UserProfileResponse?> GetProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="request">The update profile request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<UserProfileResponse> UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the current user's account.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteAccountAsync(CancellationToken cancellationToken = default);
}
