namespace OnForkHub.CrossCutting.Authentication;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;

using RefreshTokenEntity = OnForkHub.Core.Entities.RefreshToken;

/// <summary>
/// JWT token service implementation with refresh token support.
/// Refresh tokens are persisted as SHA256 hashes, rotated atomically on use,
/// bound to their owning user and protected against reuse.
/// </summary>
public sealed partial class JwtTokenService : ITokenService
{
    private readonly ILogger<JwtTokenService> _logger;
    private readonly JwtOptions _options;
    private readonly IRefreshTokenRepositoryEF _refreshTokenRepository;
    private readonly TokenValidationParameters _tokenValidationParameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="options">The JWT options.</param>
    /// <param name="refreshTokenRepository">The refresh token repository for database persistence.</param>
    /// <param name="logger">The logger.</param>
    public JwtTokenService(IOptions<JwtOptions> options, IRefreshTokenRepositoryEF refreshTokenRepository, ILogger<JwtTokenService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = _options.ValidateIssuer,
            ValidateAudience = _options.ValidateAudience,
            ValidateLifetime = _options.ValidateLifetime,
            ValidateIssuerSigningKey = _options.ValidateIssuerSigningKey,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero,
        };
    }

    /// <inheritdoc/>
    public async Task<TokenResult> GenerateTokensAsync(
        IEnumerable<Claim> claims,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(claims);

        var claimList = claims as IReadOnlyCollection<Claim> ?? claims.ToList();

        var now = DateTime.UtcNow;
        var accessTokenExpiration = now.AddMinutes(_options.AccessTokenExpirationMinutes);
        var refreshTokenExpiration = now.AddDays(_options.RefreshTokenExpirationDays);

        var accessToken = GenerateAccessToken(claimList, now, accessTokenExpiration);
        var refreshToken = GenerateRefreshToken();

        await StoreRefreshTokenAsync(refreshToken, refreshTokenExpiration, claimList, ipAddress, userAgent, cancellationToken);

        return new TokenResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = refreshTokenExpiration,
        };
    }

    /// <inheritdoc/>
    public Task<TokenResult> GenerateTokensAsync(
        string userId,
        string userName,
        IEnumerable<string>? roles = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(userName);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (roles is not null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        return GenerateTokensAsync(claims, ipAddress, userAgent, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TokenResult?> RefreshTokenAsync(
        string accessToken,
        string refreshToken,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(accessToken);
        ArgumentNullException.ThrowIfNull(refreshToken);

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal is null)
        {
            LogRefreshInvalidAccessToken();
            return null;
        }

        var tokenHash = ComputeTokenHash(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (storedToken is null)
        {
            LogRefreshUnknownToken();
            return null;
        }

        var userId = GetUserId(principal);
        if (string.IsNullOrWhiteSpace(userId) || !string.Equals(storedToken.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            LogRefreshUserMismatch(storedToken.UserId);
            await _refreshTokenRepository.RevokeAllForUserAsync(storedToken.UserId, cancellationToken);
            return null;
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        if (storedToken.RevokedAt.HasValue)
        {
            LogRefreshReuseDetected(storedToken.UserId);
            await _refreshTokenRepository.RevokeAllForUserAsync(storedToken.UserId, cancellationToken);
            return null;
        }

        var claimed = await _refreshTokenRepository.TryClaimForRotationAsync(tokenHash, cancellationToken);
        if (!claimed)
        {
            LogRefreshReuseDetected(storedToken.UserId);
            await _refreshTokenRepository.RevokeAllForUserAsync(storedToken.UserId, cancellationToken);
            return null;
        }

        return await GenerateTokensAsync(principal.Claims, ipAddress, userAgent, cancellationToken);
    }

    /// <inheritdoc/>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            LogTokenValidationFailed(ex.Message);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        var tokenHash = ComputeTokenHash(refreshToken);
        var tokenInfo = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        return tokenInfo is not null && !tokenInfo.IsRevoked;
    }

    /// <inheritdoc/>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = _options.ValidateIssuer,
                ValidateAudience = _options.ValidateAudience,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = _options.ValidateIssuerSigningKey,
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (
                securityToken is not JwtSecurityToken jwtSecurityToken
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase)
            )
            {
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            LogTokenValidationFailed(ex.Message);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        var tokenHash = ComputeTokenHash(refreshToken);
        await _refreshTokenRepository.TryClaimForRotationAsync(tokenHash, cancellationToken);
    }

    private static string ComputeTokenHash(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string? GetUserId(ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("nameid")?.Value
            ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Refresh token reuse detected for user {UserId}; all user tokens were revoked")]
    private partial void LogRefreshReuseDetected(string userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Refresh token presented for a different user than its owner {UserId}; all owner tokens were revoked")]
    private partial void LogRefreshUserMismatch(string userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Refresh attempt with unknown refresh token")]
    private partial void LogRefreshUnknownToken();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Refresh attempt with invalid access token")]
    private partial void LogRefreshInvalidAccessToken();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Token validation failed: {Reason}")]
    private partial void LogTokenValidationFailed(string reason);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to clean up expired refresh tokens for user {UserId}: {Reason}")]
    private partial void LogCleanupFailed(string userId, string reason);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot persist refresh token without a user id claim")]
    private partial void LogMissingUserIdClaim();

    private async Task StoreRefreshTokenAsync(
        string refreshToken,
        DateTime expiration,
        IReadOnlyCollection<Claim> claims,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken
    )
    {
        var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            LogMissingUserIdClaim();
            throw new InvalidOperationException("Cannot persist a refresh token without a user identifier claim.");
        }

        var refreshTokenEntity = RefreshTokenEntity.Create(
            ComputeTokenHash(refreshToken),
            userId,
            expiration,
            ipAddress ?? string.Empty,
            userAgent ?? string.Empty
        );

        if (refreshTokenEntity.Status != EResultStatus.Success || refreshTokenEntity.Data is null)
        {
            throw new InvalidOperationException($"Unable to create refresh token entity: {refreshTokenEntity.Message}");
        }

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity.Data, cancellationToken);

        try
        {
            await _refreshTokenRepository.CleanupExpiredTokensForUserAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            LogCleanupFailed(userId, ex.Message);
        }
    }

    private string GenerateAccessToken(IEnumerable<Claim> claims, DateTime now, DateTime expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
