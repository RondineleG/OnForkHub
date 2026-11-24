namespace OnForkHub.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Persistence.Contexts;
using OnForkHub.Persistence.Contexts.Base;
using OnForkHub.Persistence.Exceptions;

/// <summary>
/// Entity Framework repository implementation for RefreshToken entity.
/// Refresh tokens are persisted as SHA256 hashes only; raw tokens never reach the database.
/// </summary>
public class RefreshTokenRepositoryEF(IEntityFrameworkDataContext context) : IRefreshTokenRepositoryEF
{
    private const string EntityName = nameof(RefreshToken);
    private readonly IEntityFrameworkDataContext _context = context;

    /// <inheritdoc/>
    public async Task<RequestResult<bool>> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        try
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
            return RequestResult<bool>.Success(true);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "create", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("create", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("retrieve", $"Error retrieving refresh token: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TryClaimForRotationAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;

            if (!IsRelational())
            {
                // Portable fallback for non-relational providers (e.g. InMemory tests):
                // correct single-threaded semantics, but without the atomic database-level claim.
                var tracked = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
                if (tracked is null || tracked.RevokedAt.HasValue || tracked.ExpiresAt <= now)
                {
                    return false;
                }

                tracked.Revoke();
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            var updated = await _context
                .RefreshTokens.Where(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null && rt.ExpiresAt > now)
                .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.RevokedAt, now), cancellationToken);

            return updated == 1;
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("update", $"Error claiming refresh token for rotation: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;

            if (!IsRelational())
            {
                var tokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null).ToListAsync(cancellationToken);
                foreach (var token in tokens)
                {
                    token.Revoke();
                }

                return await _context.SaveChangesAsync(cancellationToken);
            }

            return await _context
                .RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.RevokedAt, now), cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("update", $"Error revoking all tokens for user: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> CleanupExpiredTokensForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;

            // Only naturally expired tokens are removed: revoked-but-unexpired tokens must be kept
            // as evidence so that reuse detection can revoke the whole token family.
            if (!IsRelational())
            {
                var expired = await _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.ExpiresAt <= now).ToListAsync(cancellationToken);
                _context.RefreshTokens.RemoveRange(expired);
                return await _context.SaveChangesAsync(cancellationToken);
            }

            return await _context
                .RefreshTokens.Where(rt => rt.UserId == userId && rt.ExpiresAt <= now)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("delete", $"Error cleaning up expired tokens for user: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;

            if (!IsRelational())
            {
                var expired = await _context.RefreshTokens.Where(rt => rt.ExpiresAt <= now).ToListAsync(cancellationToken);
                _context.RefreshTokens.RemoveRange(expired);
                return await _context.SaveChangesAsync(cancellationToken);
            }

            return await _context.RefreshTokens.Where(rt => rt.ExpiresAt <= now).ExecuteDeleteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("delete", $"Error cleaning up expired tokens: {ex.Message}");
        }
    }

    private bool IsRelational()
    {
        return _context is EntityFrameworkDataContext dbContext && dbContext.Database.IsRelational();
    }
}
