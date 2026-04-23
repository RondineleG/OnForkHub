namespace OnForkHub.Web.Services.Api;

using System.Threading.Tasks;

/// <summary>
/// Service contract for authentication API operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Logs in a user with email and password.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns>Auth response with tokens.</returns>
    Task<AuthResponse> LoginAsync(string email, string password);

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="name">User name.</param>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns>Auth response with tokens.</returns>
    Task<AuthResponse> RegisterAsync(string name, string email, string password);

    /// <summary>
    /// Refreshes access token using refresh token.
    /// </summary>
    /// <param name="accessToken">Expired access token.</param>
    /// <param name="refreshToken">Refresh token.</param>
    /// <returns>Auth response with new tokens.</returns>
    Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken);

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogoutAsync();
}
