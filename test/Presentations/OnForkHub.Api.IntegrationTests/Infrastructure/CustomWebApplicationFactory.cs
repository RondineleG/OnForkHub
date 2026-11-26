namespace OnForkHub.Api.IntegrationTests.Infrastructure;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using NSubstitute;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Requests;
using OnForkHub.Core.ValueObjects;
using OnForkHub.CrossCutting.Authentication;
using OnForkHub.Persistence.Contexts;
using OnForkHub.Persistence.Contexts.Base;

using Xunit;

/// <summary>
/// Custom WebApplicationFactory for xUnit integration tests.
/// Provides isolated database per test using InMemory provider.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string JwtSecret = "DevOnly_ThisIsADevelopmentKeyThatShouldNeverBeUsedInProduction_32chars!";

    private const string JwtIssuer = "OnForkHub.Api";

    private const string JwtAudience = "OnForkHub.Client";

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
                        ["Cache:UseRedis"] = "false",
                        ["Cache:DefaultExpirationMinutes"] = "30",
                        ["RateLimiting:Enabled"] = "false",
                        ["RateLimiting:PermitLimit"] = "10000",
                        ["RateLimiting:WindowSeconds"] = "60",
                        ["Jwt:SecretKey"] = JwtSecret,
                        ["Jwt:Issuer"] = JwtIssuer,
                        ["Jwt:Audience"] = JwtAudience,
                        ["Jwt:ValidateIssuer"] = "true",
                        ["Jwt:ValidateAudience"] = "true",
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

            services.AddDbContext<EntityFrameworkDataContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDatabase_{_databaseId}");
            });

            // Authentication runs end-to-end against the real services and the in-memory database:
            // register/login issue real JWTs and BCrypt-verified credentials, so integration tests
            // exercise the same pipeline as production.
        });
    }

    private static User CreateTestUser()
    {
        var id = Id.Create();
        var name = Name.Create("Test User");
        return User.Load(id, name, "test@example.com", "hashed_password", DateTime.UtcNow).Data!;
    }

    private static string GenerateTestJwt(string role = "User")
    {
        var userId = Guid.NewGuid().ToString();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Integration Test User"),
            new Claim(ClaimTypes.Role, role),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates a scoped HTTP client with a valid test authentication header.
    /// </summary>
    /// <param name="accessToken">Optional access token.</param>
    /// <returns>Configured HttpClient with Authorization header.</returns>
    public HttpClient CreateClientWithAuth(string? accessToken = null, string role = "User")
    {
        var client = CreateClient();
        var token = string.IsNullOrWhiteSpace(accessToken) ? GenerateTestJwt(role) : accessToken;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EntityFrameworkDataContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <inheritdoc/>
    Task IAsyncLifetime.DisposeAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EntityFrameworkDataContext>();
            dbContext.Database.EnsureDeleted();
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}
