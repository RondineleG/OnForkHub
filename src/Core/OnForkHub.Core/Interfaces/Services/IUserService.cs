namespace OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service interface for User operations including authentication.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new user with hashed password.
    /// </summary>
    /// <param name="name">The user's name.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="password">The user's plain text password (will be hashed).</param>
    /// <returns>The created user.</returns>
    Task<RequestResult<UserEntity>> RegisterAsync(string name, string email, string password);

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="password">The user's plain text password.</param>
    /// <returns>The authenticated user if credentials are valid.</returns>
    Task<RequestResult<UserEntity>> LoginAsync(string email, string password);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user.</returns>
    Task<RequestResult<UserEntity>> GetByIdAsync(Id id);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>The user.</returns>
    Task<RequestResult<UserEntity>> GetByEmailAsync(string email);

    /// <summary>
    /// Updates a user's profile information.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>The updated user.</returns>
    Task<RequestResult<UserEntity>> UpdateAsync(UserEntity user);

    /// <summary>
    /// Updates a user's profile by ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="name">The new name.</param>
    /// <param name="email">The new email.</param>
    /// <returns>The updated user.</returns>
    Task<RequestResult<UserEntity>> UpdateProfileAsync(string userId, string name, string email);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>Success or error result.</returns>
    Task<RequestResult> ChangePasswordAsync(Id userId, string currentPassword, string newPassword);

    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if the user exists, false otherwise.</returns>
    Task<bool> ExistsByEmailAsync(string email);
}
