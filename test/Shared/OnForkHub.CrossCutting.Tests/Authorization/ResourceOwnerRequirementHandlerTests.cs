namespace OnForkHub.CrossCutting.Tests.Authorization;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.CrossCutting.Authorization;
using OnForkHub.CrossCutting.Authorization.Handlers;
using OnForkHub.CrossCutting.Authorization.Requirements;

using System.Security.Claims;

[TestClass]
[TestCategory("Unit")]
public sealed class ResourceOwnerRequirementHandlerTests
{
    private readonly ResourceOwnerRequirementHandler _handler = new();

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerSucceedsWhenOwnerAccessesOwnResource()
    {
        var requirement = new ResourceOwnerRequirement();
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, userId);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerDoesNotSucceedWhenNonOwnerAccessesResource()
    {
        var requirement = new ResourceOwnerRequirement(allowAdminOverride: false);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, "user-456");

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerSucceedsWhenAdminAccessesWithOverrideEnabled()
    {
        var requirement = new ResourceOwnerRequirement(allowAdminOverride: true);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "admin-user"),
            new(ClaimTypes.Role, Roles.Admin),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, "different-user");

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerDoesNotSucceedWhenAdminAccessesWithOverrideDisabled()
    {
        var requirement = new ResourceOwnerRequirement(allowAdminOverride: false);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "admin-user"),
            new(ClaimTypes.Role, Roles.Admin),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, "different-user");

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerDoesNotSucceedForUnauthenticatedUser()
    {
        var requirement = new ResourceOwnerRequirement();
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, "user-123");

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerDoesNotSucceedWhenUserLacksNameIdentifierClaim()
    {
        var requirement = new ResourceOwnerRequirement();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, "user-123");

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerSucceedsWithCaseInsensitiveOwnerIdMatch()
    {
        var requirement = new ResourceOwnerRequirement();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "USER-123"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, "user-123");

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }
}
