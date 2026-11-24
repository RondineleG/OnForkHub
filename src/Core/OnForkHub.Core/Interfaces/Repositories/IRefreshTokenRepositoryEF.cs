namespace OnForkHub.Core.Interfaces.Repositories;

public interface IRefreshTokenRepositoryEF
{
    Task<RequestResult<bool>> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically revokes an active token in a single conditional UPDATE.
    /// Returns true only when this call was the one that revoked the token,
    /// which makes it suitable as a rotation claim: concurrent rotations of the
    /// same token cannot both succeed.
    /// </summary>
    Task<bool> TryClaimForRotationAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<int> RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<int> CleanupExpiredTokensForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
