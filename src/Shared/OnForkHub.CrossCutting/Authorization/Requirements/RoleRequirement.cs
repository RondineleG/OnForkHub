namespace OnForkHub.CrossCutting.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Requirement that checks if user has any of the specified roles.
/// </summary>
public sealed class RoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRequirement"/> class.
    /// </summary>
    /// <param name="allowedRoles">The roles that satisfy this requirement.</param>
    public RoleRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
    }

    /// <summary>
    /// Gets the roles that satisfy this requirement.
    /// </summary>
    public IReadOnlyList<string> AllowedRoles { get; }
}
