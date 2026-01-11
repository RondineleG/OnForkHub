namespace OnForkHub.CrossCutting.Tests.Middleware;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OnForkHub.CrossCutting.Middleware.RateLimiting;

[TestClass]
[TestCategory("Unit")]
public sealed class RateLimitingExtensionsTests
{
    [TestMethod]
    [TestCategory("RateLimiting")]
    public void AddRateLimitingServicesThrowsWhenConfigurationIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsException<ArgumentNullException>(() => services.AddRateLimitingServices(null!));
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void AddRateLimitingServicesRegistersOptionsWhenDisabled()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["RateLimiting:Enabled"] = "false" })
            .Build();

        services.AddRateLimitingServices(configuration);
        var provider = services.BuildServiceProvider();

        var options = provider.GetService<Microsoft.Extensions.Options.IOptions<RateLimitingOptions>>();
        Assert.IsNotNull(options);
        Assert.IsFalse(options.Value.Enabled);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void AddRateLimitingServicesRegistersWithCustomConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RateLimiting:Enabled"] = "true",
                    ["RateLimiting:PermitLimit"] = "200",
                    ["RateLimiting:WindowSeconds"] = "30",
                    ["RateLimiting:QueueLimit"] = "5",
                    ["RateLimiting:AuthenticatedPermitLimit"] = "1000",
                    ["RateLimiting:AnonymousPermitLimit"] = "25",
                }
            )
            .Build();

        services.AddRateLimitingServices(configuration);
        var provider = services.BuildServiceProvider();

        var options = provider.GetService<Microsoft.Extensions.Options.IOptions<RateLimitingOptions>>();
        Assert.IsNotNull(options);
        Assert.IsTrue(options.Value.Enabled);
        Assert.AreEqual(200, options.Value.PermitLimit);
        Assert.AreEqual(30, options.Value.WindowSeconds);
        Assert.AreEqual(5, options.Value.QueueLimit);
        Assert.AreEqual(1000, options.Value.AuthenticatedPermitLimit);
        Assert.AreEqual(25, options.Value.AnonymousPermitLimit);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void DefaultPolicyNameIsCorrect()
    {
        Assert.AreEqual("default", RateLimitingExtensions.DefaultPolicy);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void AuthenticatedPolicyNameIsCorrect()
    {
        Assert.AreEqual("authenticated", RateLimitingExtensions.AuthenticatedPolicy);
    }

    [TestMethod]
    [TestCategory("RateLimiting")]
    public void AnonymousPolicyNameIsCorrect()
    {
        Assert.AreEqual("anonymous", RateLimitingExtensions.AnonymousPolicy);
    }
}
