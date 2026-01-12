namespace OnForkHub.CrossCutting.Authentication;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// JWT token service implementation with refresh token support.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private static readonly ConcurrentDictionary<string, RefreshTokenInfo> RefreshTokens = new();

    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _tokenValidationParameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="options">The JWT options.</param>
    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

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

        if (!RefreshTokens.TryGetValue(refreshToken, out var tokenInfo))
        {
            return false;
        }

        if (tokenInfo.IsRevoked || tokenInfo.Expiration <= DateTime.UtcNow)
        {
            RefreshTokens.TryRemove(refreshToken, out _);
            return false;
        }

        return true;
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

        if (RefreshTokens.TryGetValue(refreshToken, out var tokenInfo))
        {
            tokenInfo.IsRevoked = true;
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static void StoreRefreshToken(string refreshToken, DateTime expiration, IEnumerable<Claim> claims)
    {
        RefreshTokens[refreshToken] = new RefreshTokenInfo
        {
            Expiration = expiration,
            Claims = claims.ToList(),
            IsRevoked = false,
        };

        CleanupExpiredTokens();
    }

    private static void CleanupExpiredTokens()
    {
        var expiredTokens = RefreshTokens
            .Where(kvp => kvp.Value.Expiration <= DateTime.UtcNow || kvp.Value.IsRevoked)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in expiredTokens)
        {
            RefreshTokens.TryRemove(token, out _);
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
