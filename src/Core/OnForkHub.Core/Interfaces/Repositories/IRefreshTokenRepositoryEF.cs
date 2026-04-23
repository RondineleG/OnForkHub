namespace OnForkHub.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing JWT refresh tokens.
/// </summary>
public interface IRefreshTokenRepositoryEF
{
    /// <summary>
    /// Creates a new refresh token in the database.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to create.</param>
    /// <returns>A request result indicating success or failure.</returns>
    Task<RequestResult<bool>> CreateAsync(RefreshToken refreshToken);

    /// <summary>
    /// Finds a refresh token by its token value.
    /// </summary>
    /// <param name="token">The token value to search for.</param>
    /// <returns>The refresh token if found, null otherwise.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Gets all active (non-revoked, non-expired) tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID to search for.</param>
    /// <returns>A collection of active refresh tokens for the user.</returns>
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    /// <param name="token">The token value to revoke.</param>
    /// <returns>True if the token was revoked successfully, false otherwise.</returns>
    Task<bool> RevokeAsync(string token);

    /// <summary>
    /// Revokes all refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID whose tokens should be revoked.</param>
    /// <returns>The number of tokens revoked.</returns>
    Task<int> RevokeAllForUserAsync(string userId);

    /// <summary>
    /// Removes expired and revoked tokens from the database (cleanup).
    /// </summary>
    /// <returns>The number of tokens removed.</returns>
    Task<int> CleanupExpiredTokensAsync();
}
