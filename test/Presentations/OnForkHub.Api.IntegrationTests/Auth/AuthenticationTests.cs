namespace OnForkHub.Api.IntegrationTests.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Application.Dtos.User.Response;

using Xunit;

public class AuthenticationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    [Trait("Authentication", "Integration")]
    public async Task Login_ShouldReturnOk_WithValidCredentials()
    {
        var request = new { Email = "test@example.com", Password = "Password123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload.RefreshToken));
    }

    [Fact]
    [Trait("Authentication", "Integration")]
    public async Task Login_ShouldReturnUnauthorized_WithInvalidCredentials()
    {
        var request = new { Email = "test@example.com", Password = "WrongPassword" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [Trait("Authentication", "Integration")]
    public async Task RefreshToken_ShouldReturnOk_WithValidRefreshToken()
    {
        var request = new { AccessToken = "expired-access-token", RefreshToken = "test-refresh-token" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    [Trait("Authentication", "Integration")]
    public async Task RefreshToken_ShouldReturnUnauthorized_WithInvalidRefreshToken()
    {
        var request = new { AccessToken = "expired-access-token", RefreshToken = "invalid-refresh-token" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [Trait("Authentication", "Integration")]
    public async Task ProtectedEndpoint_ShouldReturnUnauthorized_WithoutToken()
    {
        var response = await _client.GetAsync("/api/v1/users/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [Trait("Authentication", "Integration")]
    public async Task ProtectedEndpoint_ShouldReturnOk_WithValidToken()
    {
        var token = GenerateValidToken(Guid.NewGuid().ToString(), "Integration User");
        var client = factory.CreateClientWithAuth(token);

        var response = await client.GetAsync("/api/v1/users/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string GenerateValidToken(string userId, string userName)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Name, userName) };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DevOnly_ThisIsADevelopmentKeyThatShouldNeverBeUsedInProduction_32chars!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "OnForkHub.Api",
            audience: "OnForkHub.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
