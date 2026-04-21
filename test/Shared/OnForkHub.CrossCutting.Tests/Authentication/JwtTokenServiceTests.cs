namespace OnForkHub.CrossCutting.Tests.Authentication;

using Microsoft.Extensions.Options;
using OnForkHub.CrossCutting.Authentication;
using System.Security.Claims;

[TestClass]
[TestCategory("Unit")]
public sealed class JwtTokenServiceTests
{
    private static readonly string[] TestRoles = ["Admin", "User"];

    [TestMethod]
    [TestCategory("Authentication")]
    public void ConstructorThrowsWhenOptionsIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new JwtTokenService(null!));
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void GenerateTokensWithClaimsReturnsValidResult()
    {
        var service = CreateService();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123"), new Claim(ClaimTypes.Name, "testuser") };

        var result = service.GenerateTokens(claims);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.RefreshToken));
        Assert.AreEqual("Bearer", result.TokenType);
        Assert.IsTrue(result.AccessTokenExpiration > DateTime.UtcNow);
        Assert.IsTrue(result.RefreshTokenExpiration > DateTime.UtcNow);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void GenerateTokensWithClaimsThrowsWhenClaimsIsNull()
    {
        var service = CreateService();

        Assert.ThrowsExactly<ArgumentNullException>(() => service.GenerateTokens((IEnumerable<Claim>)null!));
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void GenerateTokensWithUserInfoReturnsValidResult()
    {
        var service = CreateService();

        var result = service.GenerateTokens("user-123", "testuser", TestRoles);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.RefreshToken));
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void GenerateTokensWithUserInfoThrowsWhenUserIdIsNull()
    {
        var service = CreateService();

        Assert.ThrowsExactly<ArgumentNullException>(() => service.GenerateTokens(null!, "testuser"));
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void GenerateTokensWithUserInfoThrowsWhenUserNameIsNull()
    {
        var service = CreateService();

        Assert.ThrowsExactly<ArgumentNullException>(() => service.GenerateTokens("user-123", null!));
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void ValidateTokenReturnsClaimsPrincipalForValidToken()
    {
        var service = CreateService();
        var result = service.GenerateTokens("user-123", "testuser");

        var principal = service.ValidateToken(result.AccessToken);

        Assert.IsNotNull(principal);
        Assert.IsNotNull(principal.Identity);
        Assert.IsTrue(principal.Identity.IsAuthenticated);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void ValidateTokenReturnsNullForInvalidToken()
    {
        var service = CreateService();

        var principal = service.ValidateToken("invalid-token");

        Assert.IsNull(principal);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void ValidateTokenReturnsNullForEmptyToken()
    {
        var service = CreateService();

        var principal = service.ValidateToken(string.Empty);

        Assert.IsNull(principal);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void ValidateRefreshTokenReturnsTrueForValidToken()
    {
        var service = CreateService();
        var result = service.GenerateTokens("user-123", "testuser");

        var isValid = service.ValidateRefreshToken(result.RefreshToken);

        Assert.IsTrue(isValid);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void ValidateRefreshTokenReturnsFalseForInvalidToken()
    {
        var service = CreateService();

        var isValid = service.ValidateRefreshToken("invalid-refresh-token");

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void ValidateRefreshTokenReturnsFalseForEmptyToken()
    {
        var service = CreateService();

        var isValid = service.ValidateRefreshToken(string.Empty);

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void RefreshTokenReturnsNewTokensForValidTokens()
    {
        var service = CreateService();
        var originalResult = service.GenerateTokens("user-123", "testuser");

        var newResult = service.RefreshToken(originalResult.AccessToken, originalResult.RefreshToken);

        Assert.IsNotNull(newResult);
        Assert.AreNotEqual(originalResult.AccessToken, newResult.AccessToken);
        Assert.AreNotEqual(originalResult.RefreshToken, newResult.RefreshToken);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void RefreshTokenReturnsNullForInvalidAccessToken()
    {
        var service = CreateService();
        var originalResult = service.GenerateTokens("user-123", "testuser");

        var newResult = service.RefreshToken("invalid-access-token", originalResult.RefreshToken);

        Assert.IsNull(newResult);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void RefreshTokenReturnsNullForInvalidRefreshToken()
    {
        var service = CreateService();
        var originalResult = service.GenerateTokens("user-123", "testuser");

        var newResult = service.RefreshToken(originalResult.AccessToken, "invalid-refresh-token");

        Assert.IsNull(newResult);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void RevokeRefreshTokenInvalidatesToken()
    {
        var service = CreateService();
        var result = service.GenerateTokens("user-123", "testuser");

        service.RevokeRefreshToken(result.RefreshToken);
        var isValid = service.ValidateRefreshToken(result.RefreshToken);

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void RevokeRefreshTokenDoesNotThrowForEmptyToken()
    {
        var service = CreateService();

        service.RevokeRefreshToken(string.Empty);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void GetPrincipalFromExpiredTokenReturnsClaimsPrincipal()
    {
        var service = CreateService();
        var result = service.GenerateTokens("user-123", "testuser");

        var principal = service.GetPrincipalFromExpiredToken(result.AccessToken);

        Assert.IsNotNull(principal);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void GetPrincipalFromExpiredTokenReturnsNullForInvalidToken()
    {
        var service = CreateService();

        var principal = service.GetPrincipalFromExpiredToken("invalid-token");

        Assert.IsNull(principal);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void GetPrincipalFromExpiredTokenReturnsNullForEmptyToken()
    {
        var service = CreateService();

        var principal = service.GetPrincipalFromExpiredToken(string.Empty);

        Assert.IsNull(principal);
    }

    private static JwtTokenService CreateService(JwtOptions? options = null)
    {
        options ??= new JwtOptions
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

        return new JwtTokenService(Options.Create(options));
    }
}
