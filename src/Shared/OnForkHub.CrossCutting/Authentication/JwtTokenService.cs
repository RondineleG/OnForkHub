namespace OnForkHub.CrossCutting.Authentication;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Repositories;

using RefreshTokenEntity = OnForkHub.Core.Entities.RefreshToken;

/// <summary>
/// JWT token service implementation with refresh token support.
/// Refresh tokens are persisted in the database for scalability and reliability.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly IRefreshTokenRepositoryEF _refreshTokenRepository;
    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _tokenValidationParameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="options">The JWT options.</param>
    /// <param name="refreshTokenRepository">The refresh token repository for database persistence.</param>
    public JwtTokenService(IOptions<JwtOptions> options, IRefreshTokenRepositoryEF refreshTokenRepository)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));

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
    public TokenResult GenerateTokens(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        var now = DateTime.UtcNow;
        var accessTokenExpiration = now.AddMinutes(_options.AccessTokenExpirationMinutes);
        var refreshTokenExpiration = now.AddDays(_options.RefreshTokenExpirationDays);

        var accessToken = GenerateAccessToken(claims, now, accessTokenExpiration);
        var refreshToken = GenerateRefreshToken();

        StoreRefreshToken(refreshToken, refreshTokenExpiration, claims);

        return new TokenResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = refreshTokenExpiration,
        };
    }

    /// <summary>
    /// Generates JWT tokens with IP address and user agent tracking.
    /// </summary>
    /// <param name="claims">The user claims.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <param name="userAgent">The user agent string of the client.</param>
    /// <returns>A token result containing access and refresh tokens.</returns>
    public TokenResult GenerateTokens(IEnumerable<Claim> claims, string? ipAddress, string? userAgent)
    {
        ArgumentNullException.ThrowIfNull(claims);

        var now = DateTime.UtcNow;
        var accessTokenExpiration = now.AddMinutes(_options.AccessTokenExpirationMinutes);
        var refreshTokenExpiration = now.AddDays(_options.RefreshTokenExpirationDays);

        var accessToken = GenerateAccessToken(claims, now, accessTokenExpiration);
        var refreshToken = GenerateRefreshToken();

        StoreRefreshToken(refreshToken, refreshTokenExpiration, claims, ipAddress, userAgent);

        return new TokenResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = refreshTokenExpiration,
        };
    }

    /// <inheritdoc/>
    public TokenResult GenerateTokens(string userId, string userName, IEnumerable<string>? roles = null)
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

        return GenerateTokens(claims);
    }

    /// <inheritdoc/>
    public TokenResult? RefreshToken(string accessToken, string refreshToken)
    {
        ArgumentNullException.ThrowIfNull(accessToken);
        ArgumentNullException.ThrowIfNull(refreshToken);

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal is null)
        {
            return null;
        }

        if (!ValidateRefreshToken(refreshToken))
        {
            return null;
        }

        RevokeRefreshToken(refreshToken);

        return GenerateTokens(principal.Claims);
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
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public bool ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        try
        {
            var tokenInfo = _refreshTokenRepository.GetByTokenAsync(refreshToken).GetAwaiter().GetResult();

            if (tokenInfo == null)
            {
                return false;
            }

            if (tokenInfo.IsRevoked)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
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
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public void RevokeRefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        try
        {
            _refreshTokenRepository.RevokeAsync(refreshToken).GetAwaiter().GetResult();
        }
        catch
        {
            // Silently fail - token may not exist
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private void StoreRefreshToken(
        string refreshToken,
        DateTime expiration,
        IEnumerable<Claim> claims,
        string? ipAddress = null,
        string? userAgent = null
    )
    {
        try
        {
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // Cannot store token without user ID - log this as a warning
                return;
            }

            var refreshTokenEntity = RefreshTokenEntity.Create(
                refreshToken,
                userId,
                expiration,
                ipAddress ?? string.Empty,
                userAgent ?? string.Empty
            );

            if (refreshTokenEntity.Status == EResultStatus.Success)
            {
                _refreshTokenRepository.CreateAsync(refreshTokenEntity.Data!).GetAwaiter().GetResult();
            }

            CleanupExpiredTokens();
        }
        catch
        {
            // Silently fail - logging should be done in production
        }
    }

    private void CleanupExpiredTokens()
    {
        try
        {
            _refreshTokenRepository.CleanupExpiredTokensAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Silently fail - cleanup is not critical
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

    private sealed class RefreshTokenInfo
    {
        public DateTime Expiration { get; init; }

        public List<Claim> Claims { get; init; } = [];

        public bool IsRevoked { get; set; }
    }
}
