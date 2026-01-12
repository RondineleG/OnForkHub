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
    /// <returns>A token result containing access and refresh tokens.</returns>
    TokenResult GenerateTokens(IEnumerable<Claim> claims);

    /// <summary>
    /// Generates a new access token and refresh token for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="roles">The user roles.</param>
    /// <returns>A token result containing access and refresh tokens.</returns>
    TokenResult GenerateTokens(string userId, string userName, IEnumerable<string>? roles = null);

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="accessToken">The expired access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>A new token result if refresh is successful, null otherwise.</returns>
    TokenResult? RefreshToken(string accessToken, string refreshToken);

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
    /// <returns>True if the refresh token is valid, false otherwise.</returns>
    bool ValidateRefreshToken(string refreshToken);

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
    void RevokeRefreshToken(string refreshToken);
}
