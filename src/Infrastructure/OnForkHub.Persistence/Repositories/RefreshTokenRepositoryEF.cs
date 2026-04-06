namespace OnForkHub.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Persistence.Contexts.Base;
using OnForkHub.Persistence.Exceptions;

/// <summary>
/// Entity Framework repository implementation for RefreshToken entity.
/// </summary>
public class RefreshTokenRepositoryEF(IEntityFrameworkDataContext context) : IRefreshTokenRepositoryEF
{
    private const string EntityName = nameof(RefreshToken);
    private readonly IEntityFrameworkDataContext _context = context;

    /// <inheritdoc/>
    public async Task<RequestResult<bool>> CreateAsync(RefreshToken refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        try
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
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
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        try
        {
            var refreshToken = await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token);

            return refreshToken;
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("retrieve", $"Error retrieving refresh token: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        try
        {
            var tokens = await _context.RefreshTokens
                .AsNoTracking()
                .Where(rt => rt.UserId == userId && !rt.RevokedAt.HasValue && rt.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();

            return tokens.AsReadOnly();
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("retrieve", $"Error retrieving refresh tokens for user: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RevokeAsync(string token)
    {
        try
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null)
            {
                return false;
            }

            refreshToken.Revoke();
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "update", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("update", $"Error revoking refresh token: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> RevokeAllForUserAsync(string userId)
    {
        try
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.RevokedAt.HasValue)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoke();
            }

            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "update", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("update", $"Error revoking all tokens for user: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> CleanupExpiredTokensAsync()
    {
        try
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.RevokedAt.HasValue)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "delete", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("delete", $"Error cleaning up expired tokens: {ex.Message}");
        }
    }
}
