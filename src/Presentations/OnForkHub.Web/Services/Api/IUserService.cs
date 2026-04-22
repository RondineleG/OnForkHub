namespace OnForkHub.Web.Services.Api;

using OnForkHub.Core.Requests.Users;
using OnForkHub.Core.Responses.Users;

/// <summary>
/// Service contract for user API operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User profile response.</returns>
    Task<UserProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user profile response.</returns>
    Task<UserProfileResponse> UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}
