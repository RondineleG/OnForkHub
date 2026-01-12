namespace OnForkHub.CrossCutting.Tests.Authorization;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.CrossCutting.Authorization;
using OnForkHub.CrossCutting.Authorization.Handlers;
using OnForkHub.CrossCutting.Authorization.Requirements;

using System.Security.Claims;

[TestClass]
[TestCategory("Unit")]
public sealed class RoleRequirementHandlerTests
{
    private readonly RoleRequirementHandler _handler = new();

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerSucceedsWhenUserHasMatchingRole()
    {
        var requirement = new RoleRequirement(Roles.Admin);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-1"),
            new(ClaimTypes.Role, Roles.Admin),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerDoesNotSucceedWhenUserLacksMatchingRole()
    {
        var requirement = new RoleRequirement(Roles.Admin);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-1"),
            new(ClaimTypes.Role, Roles.User),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerDoesNotSucceedForUnauthenticatedUser()
    {
        var requirement = new RoleRequirement(Roles.Admin);
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerSucceedsWhenUserHasAnyAllowedRole()
    {
        var requirement = new RoleRequirement(Roles.Admin, Roles.Moderator);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-1"),
            new(ClaimTypes.Role, Roles.Moderator),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerSucceedsWithCaseInsensitiveRoleMatch()
    {
        var requirement = new RoleRequirement("ADMIN");
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-1"),
            new(ClaimTypes.Role, "admin"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public async Task HandlerDoesNotSucceedWithEmptyRolesRequirement()
    {
        var requirement = new RoleRequirement();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-1"),
            new(ClaimTypes.Role, Roles.Admin),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
