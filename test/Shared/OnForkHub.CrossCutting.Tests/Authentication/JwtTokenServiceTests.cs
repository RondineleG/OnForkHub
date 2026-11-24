namespace OnForkHub.CrossCutting.Tests.Authentication;

using System.Security.Claims;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Requests;
using OnForkHub.CrossCutting.Authentication;

[TestClass]
[TestCategory("Unit")]
public sealed class JwtTokenServiceTests
{
    private static readonly string[] TestRoles = ["Admin", "User"];

    [TestMethod]
    [TestCategory("Authentication")]
    public void ConstructorThrowsWhenOptionsIsNull()
    {
        var repository = new InMemoryRefreshTokenRepository();
        Assert.ThrowsExactly<ArgumentNullException>(() => new JwtTokenService(null!, repository, NullLogger<JwtTokenService>.Instance));
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task GenerateTokensWithClaimsReturnsValidResult()
    {
        var service = CreateService(out _);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123"), new Claim(ClaimTypes.Name, "testuser") };

        var result = await service.GenerateTokensAsync(claims);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.RefreshToken));
        Assert.AreEqual("Bearer", result.TokenType);
        Assert.IsTrue(result.AccessTokenExpiration > DateTime.UtcNow);
        Assert.IsTrue(result.RefreshTokenExpiration > DateTime.UtcNow);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task GenerateTokensPersistsOnlyTheHashOfTheRefreshToken()
    {
        var service = CreateService(out var repository);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };

        var result = await service.GenerateTokensAsync(claims);

        var stored = repository.Tokens.Values.Single();
        Assert.AreNotEqual(result.RefreshToken, stored.TokenHash, "The raw refresh token must never be persisted");
        Assert.AreEqual(64, stored.TokenHash.Length, "Stored token hash should be a lowercase SHA256 hex digest");
        Assert.AreEqual(stored.TokenHash, stored.TokenHash.ToLowerInvariant());
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task GenerateTokensThrowsWhenClaimsHaveNoUserId()
    {
        var service = CreateService(out _);
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => service.GenerateTokensAsync(claims));
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task GenerateTokensWithUserInfoReturnsValidResult()
    {
        var service = CreateService(out _);

        var result = await service.GenerateTokensAsync("user-123", "testuser", TestRoles);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.RefreshToken));
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task ValidateTokenReturnsClaimsPrincipalForValidToken()
    {
        var service = CreateService(out _);
        var result = await service.GenerateTokensAsync("user-123", "testuser");

        var principal = service.ValidateToken(result.AccessToken);

        Assert.IsNotNull(principal);
        Assert.IsNotNull(principal.Identity);
        Assert.IsTrue(principal.Identity.IsAuthenticated);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void ValidateTokenReturnsNullForInvalidToken()
    {
        var service = CreateService(out _);

        var principal = service.ValidateToken("invalid-token");

        Assert.IsNull(principal);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task ValidateRefreshTokenReturnsTrueForValidToken()
    {
        var service = CreateService(out _);
        var result = await service.GenerateTokensAsync("user-123", "testuser");

        var isValid = await service.ValidateRefreshTokenAsync(result.RefreshToken);

        Assert.IsTrue(isValid);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task ValidateRefreshTokenReturnsFalseForInvalidToken()
    {
        var service = CreateService(out _);

        var isValid = await service.ValidateRefreshTokenAsync("invalid-refresh-token");

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task RefreshTokenRotatesAndInvalidatesTheOriginalToken()
    {
        var service = CreateService(out _);
        var originalResult = await service.GenerateTokensAsync("user-123", "testuser");

        var newResult = await service.RefreshTokenAsync(originalResult.AccessToken, originalResult.RefreshToken);

        Assert.IsNotNull(newResult);
        Assert.AreNotEqual(originalResult.AccessToken, newResult.AccessToken);
        Assert.AreNotEqual(originalResult.RefreshToken, newResult.RefreshToken);

        var originalStillValid = await service.ValidateRefreshTokenAsync(originalResult.RefreshToken);
        Assert.IsFalse(originalStillValid, "The original refresh token must be burned after rotation");
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task RefreshTokenReuseRevokesAllUserTokens()
    {
        var service = CreateService(out _);
        var originalResult = await service.GenerateTokensAsync("user-123", "testuser");

        var rotated = await service.RefreshTokenAsync(originalResult.AccessToken, originalResult.RefreshToken);
        Assert.IsNotNull(rotated);

        // Attacker (or race) presents the already-rotated original token again
        var replay = await service.RefreshTokenAsync(originalResult.AccessToken, originalResult.RefreshToken);

        Assert.IsNull(replay, "Reusing a rotated refresh token must fail");

        var rotatedStillValid = await service.ValidateRefreshTokenAsync(rotated.RefreshToken);
        Assert.IsFalse(rotatedStillValid, "Reuse detection must revoke the whole token family");
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task RefreshTokenFailsWhenAccessTokenBelongsToAnotherUser()
    {
        var service = CreateService(out _);
        var userA = await service.GenerateTokensAsync("user-a", "usera");
        var userB = await service.GenerateTokensAsync("user-b", "userb");

        var result = await service.RefreshTokenAsync(userB.AccessToken, userA.RefreshToken);

        Assert.IsNull(result, "A refresh token must not mint tokens for a different user");

        var ownerTokensRevoked = await service.ValidateRefreshTokenAsync(userA.RefreshToken);
        Assert.IsFalse(ownerTokensRevoked, "Cross-user usage must revoke the token owner's family");
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task RefreshTokenReturnsNullForInvalidAccessToken()
    {
        var service = CreateService(out _);
        var originalResult = await service.GenerateTokensAsync("user-123", "testuser");

        var newResult = await service.RefreshTokenAsync("invalid-access-token", originalResult.RefreshToken);

        Assert.IsNull(newResult);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task RefreshTokenReturnsNullForUnknownRefreshToken()
    {
        var service = CreateService(out _);
        var originalResult = await service.GenerateTokensAsync("user-123", "testuser");

        var newResult = await service.RefreshTokenAsync(originalResult.AccessToken, "invalid-refresh-token");

        Assert.IsNull(newResult);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task RevokeRefreshTokenInvalidatesToken()
    {
        var service = CreateService(out _);
        var result = await service.GenerateTokensAsync("user-123", "testuser");

        await service.RevokeRefreshTokenAsync(result.RefreshToken);
        var isValid = await service.ValidateRefreshTokenAsync(result.RefreshToken);

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task RevokeRefreshTokenDoesNotThrowForEmptyToken()
    {
        var service = CreateService(out _);

        await service.RevokeRefreshTokenAsync(string.Empty);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public async Task GetPrincipalFromExpiredTokenReturnsClaimsPrincipal()
    {
        var service = CreateService(out _);
        var result = await service.GenerateTokensAsync("user-123", "testuser");

        var principal = service.GetPrincipalFromExpiredToken(result.AccessToken);

        Assert.IsNotNull(principal);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void GetPrincipalFromExpiredTokenReturnsNullForInvalidToken()
    {
        var service = CreateService(out _);

        var principal = service.GetPrincipalFromExpiredToken("invalid-token");

        Assert.IsNull(principal);
    }

    private static JwtTokenService CreateService(out InMemoryRefreshTokenRepository repository)
    {
        var options = new JwtOptions
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposes12345",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };

        repository = new InMemoryRefreshTokenRepository();
        return new JwtTokenService(Options.Create(options), repository, NullLogger<JwtTokenService>.Instance);
    }

    /// <summary>
    /// In-memory implementation mirroring the repository contract, including atomic rotation semantics.
    /// </summary>
    private sealed class InMemoryRefreshTokenRepository : IRefreshTokenRepositoryEF
    {
        public Dictionary<string, RefreshToken> Tokens { get; } = [];

        public Task<RequestResult<bool>> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            Tokens[refreshToken.TokenHash] = refreshToken;
            return Task.FromResult(RequestResult<bool>.Success(true));
        }

        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tokens.TryGetValue(tokenHash, out var rt) ? rt : (RefreshToken?)null);
        }

        public Task<bool> TryClaimForRotationAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            lock (Tokens)
            {
                if (!Tokens.TryGetValue(tokenHash, out var rt) || rt.RevokedAt.HasValue || rt.ExpiresAt <= DateTime.UtcNow)
                {
                    return Task.FromResult(false);
                }

                rt.Revoke();
                return Task.FromResult(true);
            }
        }

        public Task<int> RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var count = 0;
            foreach (var token in Tokens.Values.Where(t => t.UserId == userId && !t.RevokedAt.HasValue))
            {
                token.Revoke();
                count++;
            }

            return Task.FromResult(count);
        }

        public Task<int> CleanupExpiredTokensForUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expired = Tokens.Values.Where(t => t.UserId == userId && t.ExpiresAt <= now).Select(t => t.TokenHash).ToList();
            foreach (var key in expired)
            {
                Tokens.Remove(key);
            }

            return Task.FromResult(expired.Count);
        }

        public Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expired = Tokens.Values.Where(t => t.ExpiresAt <= now).Select(t => t.TokenHash).ToList();
            foreach (var key in expired)
            {
                Tokens.Remove(key);
            }

            return Task.FromResult(expired.Count);
        }
    }
}
