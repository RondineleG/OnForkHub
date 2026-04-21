namespace OnForkHub.Api.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Api.Extensions;
using OnForkHub.Persistence.Contexts;

using Xunit;

/// <summary>
/// Custom WebApplicationFactory for xUnit integration tests.
/// Provides isolated database per test using InMemory provider.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseId;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomWebApplicationFactory"/> class.
    /// </summary>
    public CustomWebApplicationFactory()
    {
        _databaseId = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Gets the unique database identifier for this factory instance.
    /// </summary>
    public string DatabaseId => _databaseId;

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["AppSettings:ApiMode"] = "Rest",
                        ["Cache:UseRedis"] = "false",
                        ["Cache:DefaultExpirationMinutes"] = "30",
                        ["RateLimiting:Enabled"] = "false",
                        ["RateLimiting:PermitLimit"] = "10000",
                        ["RateLimiting:WindowSeconds"] = "60",
                        ["Jwt:SecretKey"] = "TestSecretKey_ForIntegrationTests_Only_32chars!!",
                        ["Jwt:Issuer"] = "TestIssuer",
                        ["Jwt:Audience"] = "TestAudience",
                        ["Jwt:ValidateIssuer"] = "false",
                        ["Jwt:ValidateAudience"] = "false",
                        ["Jwt:ValidateLifetime"] = "true",
                        ["Jwt:ValidateIssuerSigningKey"] = "true",
                        ["Jwt:AccessTokenExpirationMinutes"] = "15",
                        ["Jwt:RefreshTokenExpirationDays"] = "7",
                    }
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            services.AddEndpoints();

            // Remove all DbContext and DbContextOptions registrations
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType.Name.Contains("DbContext")
                    || d.ServiceType.Name.Contains("DbContextOptions")
                    || d.ServiceType == typeof(EntityFrameworkDataContext)
                )
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing with unique database per factory
            services.AddDbContext<EntityFrameworkDataContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDatabase_{_databaseId}");
            });
        });
    }

    /// <summary>
    /// Creates a scoped HTTP client with optional default authorization header.
    /// </summary>
    /// <param name="accessToken">Optional JWT access token.</param>
    /// <returns>Configured HttpClient.</returns>
    public HttpClient CreateClientWithAuth(string? accessToken = null)
    {
        var client = CreateClient();

        if (!string.IsNullOrEmpty(accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        return client;
    }

    /// <summary>
    /// Creates a service scope for direct service access.
    /// </summary>
    /// <returns>Async service scope.</returns>
    public AsyncServiceScope CreateScopeAsync()
    {
        var scope = Services.CreateAsyncScope();
        return scope;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        // Ensure the application is built
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EntityFrameworkDataContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <inheritdoc/>
    Task IAsyncLifetime.DisposeAsync()
    {
        // Clean up in-memory database
        try
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EntityFrameworkDataContext>();
            dbContext.Database.EnsureDeleted();
        }
        catch
        {
            // Ignore cleanup errors during disposal
        }

        return Task.CompletedTask;
    }
}
