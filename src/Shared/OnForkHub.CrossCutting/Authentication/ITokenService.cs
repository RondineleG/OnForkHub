namespace OnForkHub.CrossCutting.Authentication;

using System.Security.Claims;

/// <summary>
/// Defines a contract for JWT token operations.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a new access token and refresh token for the specified claims.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <param name="ipAddress">The IP address of the client, if available.</param>
    /// <param name="userAgent">The user agent string of the client, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A token result containing access and refresh tokens.</returns>
    Task<TokenResult> GenerateTokensAsync(
        IEnumerable<Claim> claims,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Generates a new access token and refresh token for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="roles">The user roles.</param>
    /// <param name="ipAddress">The IP address of the client, if available.</param>
    /// <param name="userAgent">The user agent string of the client, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A token result containing access and refresh tokens.</returns>
    Task<TokenResult> GenerateTokensAsync(
        string userId,
        string userName,
        IEnumerable<string>? roles = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Performs rotation, user binding validation and reuse detection.
    /// </summary>
    /// <param name="accessToken">The expired access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="ipAddress">The IP address of the client, if available.</param>
    /// <param name="userAgent">The user agent string of the client, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A new token result if refresh is successful, null otherwise.</returns>
    Task<TokenResult?> RefreshTokenAsync(
        string accessToken,
        string refreshToken,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Validates an access token and returns the claims principal.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The claims principal if valid, null otherwise.</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Validates a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the refresh token is valid, false otherwise.</returns>
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the principal from an expired token without validating lifetime.
    /// </summary>
    /// <param name="token">The expired token.</param>
    /// <returns>The claims principal if the token structure is valid, null otherwise.</returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
