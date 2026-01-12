namespace OnForkHub.CrossCutting.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Requirement that checks if user owns the resource or has elevated permissions.
/// </summary>
public sealed class ResourceOwnerRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceOwnerRequirement"/> class.
    /// </summary>
    /// <param name="allowAdminOverride">Whether admins can override ownership check.</param>
    public ResourceOwnerRequirement(bool allowAdminOverride = true)
    {
        AllowAdminOverride = allowAdminOverride;
    }

    /// <summary>
    /// Gets a value indicating whether admins can bypass ownership check.
    /// </summary>
    public bool AllowAdminOverride { get; }
}
