namespace OnForkHub.CrossCutting.Authorization.Handlers;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.CrossCutting.Authorization.Requirements;

using System.Security.Claims;

/// <summary>
/// Handler for role-based authorization requirements.
/// </summary>
public sealed class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        var userRoles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (requirement.AllowedRoles.Any(role =>
            userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
