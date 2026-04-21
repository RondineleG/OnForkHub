namespace OnForkHub.CrossCutting.Tests.Authentication;

using OnForkHub.CrossCutting.Authentication;

[TestClass]
[TestCategory("Unit")]
public sealed class JwtOptionsTests
{
    [TestMethod]
    [TestCategory("Authentication")]
    public void SectionNameReturnsCorrectValue()
    {
        Assert.AreEqual("Jwt", JwtOptions.SectionName);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void DefaultValuesAreCorrectlySet()
    {
        var options = new JwtOptions();

        Assert.AreEqual(string.Empty, options.SecretKey);
        Assert.AreEqual(string.Empty, options.Issuer);
        Assert.AreEqual(string.Empty, options.Audience);
        Assert.AreEqual(15, options.AccessTokenExpirationMinutes);
        Assert.AreEqual(7, options.RefreshTokenExpirationDays);
        Assert.IsTrue(options.ValidateIssuer);
        Assert.IsTrue(options.ValidateAudience);
        Assert.IsTrue(options.ValidateLifetime);
        Assert.IsTrue(options.ValidateIssuerSigningKey);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void CanSetSecretKeyProperty()
    {
        var options = new JwtOptions { SecretKey = "test-secret-key-12345678901234567890" };

        Assert.AreEqual("test-secret-key-12345678901234567890", options.SecretKey);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void CanSetIssuerProperty()
    {
        var options = new JwtOptions { Issuer = "test-issuer" };

        Assert.AreEqual("test-issuer", options.Issuer);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void CanSetAudienceProperty()
    {
        var options = new JwtOptions { Audience = "test-audience" };

        Assert.AreEqual("test-audience", options.Audience);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void CanSetAccessTokenExpirationMinutesProperty()
    {
        var options = new JwtOptions { AccessTokenExpirationMinutes = 30 };

        Assert.AreEqual(30, options.AccessTokenExpirationMinutes);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void CanSetRefreshTokenExpirationDaysProperty()
    {
        var options = new JwtOptions { RefreshTokenExpirationDays = 14 };

        Assert.AreEqual(14, options.RefreshTokenExpirationDays);
    }

    [TestMethod]
    [TestCategory("Authentication")]
    public void CanSetValidationProperties()
    {
        var options = new JwtOptions
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
        };

        Assert.IsFalse(options.ValidateIssuer);
        Assert.IsFalse(options.ValidateAudience);
        Assert.IsFalse(options.ValidateLifetime);
        Assert.IsFalse(options.ValidateIssuerSigningKey);
    }
}
