namespace OnForkHub.CrossCutting.Authorization.Handlers;

using Microsoft.AspNetCore.Authorization;
using OnForkHub.CrossCutting.Authorization.Requirements;
using System.Security.Claims;

/// <summary>
/// Handler for resource ownership authorization requirements.
/// </summary>
public sealed class ResourceOwnerRequirementHandler : AuthorizationHandler<ResourceOwnerRequirement, string>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceOwnerRequirement requirement, string resourceOwnerId)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);
        ArgumentNullException.ThrowIfNull(resourceOwnerId);

        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Task.CompletedTask;
        }

        if (userId.Equals(resourceOwnerId, StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (requirement.AllowAdminOverride && IsAdmin(context.User))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool IsAdmin(ClaimsPrincipal user)
    {
        return user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase));
    }
}
