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

using Raven.Client.Documents;

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
                        ["RavenDbSettings:Urls:0"] = "http://localhost:8080",
                        ["RavenDbSettings:Database"] = "OnForkHub_Test",
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
                    || d.ServiceType == typeof(IDocumentStore)
                    || d.ServiceType == typeof(ICategoryServiceRavenDB)
                    || d.ServiceType == typeof(IUserService)
                    || d.ServiceType == typeof(ITokenService)
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

            var mockStore = Substitute.For<IDocumentStore>();
            services.AddSingleton(mockStore);

            var mockRavenService = Substitute.For<ICategoryServiceRavenDB>();
            mockRavenService.CreateAsync(Arg.Any<Category>()).Returns(args => Task.FromResult(RequestResult<Category>.Success((Category)args[0])));
            services.AddScoped(_ => mockRavenService);

            var mockTokenService = Substitute.For<ITokenService>();
            var tokenResult = new TokenResult
            {
                AccessToken = GenerateTestJwt(),
                RefreshToken = "test-refresh-token",
                AccessTokenExpiration = DateTime.UtcNow.AddHours(1),
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(7),
            };
            mockTokenService.GenerateTokens(Arg.Any<IEnumerable<Claim>>()).Returns(tokenResult);
            mockTokenService.GenerateTokens(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>()).Returns(tokenResult);
            mockTokenService.RefreshToken(Arg.Any<string>(), Arg.Is<string>(x => x == "test-refresh-token")).Returns(tokenResult);
            mockTokenService.RefreshToken(Arg.Any<string>(), Arg.Is<string>(x => x != "test-refresh-token")).Returns((TokenResult?)null);
            services.AddScoped(_ => mockTokenService);

            var mockUserService = Substitute.For<IUserService>();
            mockUserService
                .LoginAsync(Arg.Any<string>(), Arg.Is<string>(x => x == "Password123!"))
                .Returns(Task.FromResult(RequestResult<User>.Success(CreateTestUser())));
            mockUserService
                .LoginAsync(Arg.Any<string>(), Arg.Is<string>(x => x != "Password123!"))
                .Returns(Task.FromResult(RequestResult<User>.WithError("Invalid credentials")));
            mockUserService
                .RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(RequestResult<User>.Success(CreateTestUser())));
            mockUserService.GetByIdAsync(Arg.Any<Id>()).Returns(Task.FromResult(RequestResult<User>.Success(CreateTestUser())));
            services.AddScoped(_ => mockUserService);
        });
    }

    private static User CreateTestUser()
    {
        var id = Id.Create();
        var name = Name.Create("Test User");
        return User.Load(id, name, "test@example.com", "hashed_password", DateTime.UtcNow).Data!;
    }

    private static string GenerateTestJwt()
    {
        var userId = Guid.NewGuid().ToString();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Integration Test User"),
            new Claim(ClaimTypes.Role, "User"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestSecretKey_ForIntegrationTests_Only_32chars!!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
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
    public HttpClient CreateClientWithAuth(string? accessToken = null)
    {
        var client = CreateClient();
        var token = string.IsNullOrWhiteSpace(accessToken) ? GenerateTestJwt() : accessToken;
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
