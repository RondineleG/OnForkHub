namespace OnForkHub.CrossCutting.Tests.Middleware;

using OnForkHub.CrossCutting.Middleware.RateLimiting;

[TestClass]
[TestCategory("Unit")]
public sealed class RateLimitingOptionsTests
{
    [TestMethod]
    [TestCategory("RateLimiting")]
    public void SectionNameReturnsCorrectValue()
    {
        Assert.AreEqual("RateLimiting", RateLimitingOptions.SectionName);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void DefaultValuesAreCorrectlySet()
    {
        var options = new RateLimitingOptions();

        Assert.IsTrue(options.Enabled);
        Assert.AreEqual(100, options.PermitLimit);
        Assert.AreEqual(60, options.WindowSeconds);
        Assert.AreEqual(10, options.QueueLimit);
        Assert.AreEqual(500, options.AuthenticatedPermitLimit);
        Assert.AreEqual(50, options.AnonymousPermitLimit);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void CanSetEnabledProperty()
    {
        var options = new RateLimitingOptions { Enabled = false };

        Assert.IsFalse(options.Enabled);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void CanSetPermitLimitProperty()
    {
        var options = new RateLimitingOptions { PermitLimit = 200 };

        Assert.AreEqual(200, options.PermitLimit);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void CanSetWindowSecondsProperty()
    {
        var options = new RateLimitingOptions { WindowSeconds = 120 };

        Assert.AreEqual(120, options.WindowSeconds);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void CanSetQueueLimitProperty()
    {
        var options = new RateLimitingOptions { QueueLimit = 20 };

        Assert.AreEqual(20, options.QueueLimit);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void CanSetAuthenticatedPermitLimitProperty()
    {
        var options = new RateLimitingOptions { AuthenticatedPermitLimit = 1000 };

        Assert.AreEqual(1000, options.AuthenticatedPermitLimit);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void CanSetAnonymousPermitLimitProperty()
    {
        var options = new RateLimitingOptions { AnonymousPermitLimit = 25 };

        Assert.AreEqual(25, options.AnonymousPermitLimit);
    }
}
