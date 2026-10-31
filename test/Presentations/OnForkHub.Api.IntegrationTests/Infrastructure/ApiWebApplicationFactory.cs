namespace OnForkHub.Api.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Persistence.Contexts;

/// <summary>
/// Custom WebApplicationFactory for API integration tests.
/// </summary>
internal sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Configures the web host for testing.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Cache:UseRedis"] = "false",
                        ["Cache:DefaultExpirationMinutes"] = "30",
                        ["RateLimiting:Enabled"] = "true",
                        ["RateLimiting:PermitLimit"] = "1000",
                        ["RateLimiting:WindowSeconds"] = "60",
                    }
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType.Name.Contains("DbContext")
                    || d.ServiceType.Name.Contains("DbContextOptions")
                    || d.ServiceType == typeof(DbContextOptions<EntityFrameworkDataContext>)
                )
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<EntityFrameworkDataContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}");
            });
        });
    }
}
