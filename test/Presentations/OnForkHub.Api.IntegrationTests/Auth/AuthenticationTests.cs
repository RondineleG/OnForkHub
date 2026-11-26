namespace OnForkHub.Api.IntegrationTests.Auth;

using System.Net;
using System.Net.Http.Json;

using OnForkHub.Api.IntegrationTests.Infrastructure;

using Xunit;

public class AuthenticationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Login_ShouldReturnOk_WhenUserRegisteredWithSameCredentials()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        const string password = "Str0ng!Password";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Name = "Login Test User", Email = email, Password = password });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = password });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        var email = $"wrongpass-{Guid.NewGuid():N}@example.com";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new { Name = "Wrong Pass User", Email = email, Password = "Str0ng!Password" });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Different!Passw0rd" });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { Email = $"ghost-{Guid.NewGuid():N}@example.com", Password = "Str0ng!Password" });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Register_ShouldReturnConflictOrError_WhenEmailAlreadyRegistered()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var payload = new { Name = "Dup User", Email = email, Password = "Str0ng!Password" };

        var first = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.NotEqual(HttpStatusCode.Created, second.StatusCode);
        Assert.NotEqual(HttpStatusCode.OK, second.StatusCode);
    }
}
