namespace OnForkHub.CrossCutting.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OnForkHub.CrossCutting.Authorization.Handlers;
using OnForkHub.CrossCutting.Authorization.Requirements;

/// <summary>
/// Extension methods for configuring authorization services.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds role-based authorization with predefined policies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRoleBasedAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IAuthorizationHandler, RoleRequirementHandler>();
        services.AddSingleton<IAuthorizationHandler, ResourceOwnerRequirementHandler>();

        services
            .AddAuthorizationBuilder()
            .AddPolicy(Policies.RequireAdmin, policy => policy.AddRequirements(new RoleRequirement(Roles.Admin)))
            .AddPolicy(Policies.RequireModerator, policy => policy.AddRequirements(new RoleRequirement(Roles.Admin, Roles.Moderator)))
            .AddPolicy(Policies.RequireUser, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(Policies.RequirePremium, policy => policy.AddRequirements(new RoleRequirement(Roles.Admin, Roles.Premium)))
            .AddPolicy(Policies.CanManageContent, policy => policy.AddRequirements(new RoleRequirement(Roles.Admin, Roles.Moderator)))
            .AddPolicy(Policies.CanManageUsers, policy => policy.AddRequirements(new RoleRequirement(Roles.Admin)))
            .AddPolicy(Policies.CanUploadVideos, policy => policy.AddRequirements(new RoleRequirement(Roles.Admin, Roles.Moderator, Roles.User)))
            .AddPolicy(Policies.CanViewPremiumContent, policy => policy.AddRequirements(new RoleRequirement(Roles.Admin, Roles.Premium)));

        return services;
    }

    /// <summary>
    /// Adds a custom authorization policy.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="policyName">The policy name.</param>
    /// <param name="allowedRoles">The allowed roles.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCustomPolicy(this IServiceCollection services, string policyName, params string[] allowedRoles)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(policyName);
        ArgumentNullException.ThrowIfNull(allowedRoles);

        services.AddAuthorizationBuilder().AddPolicy(policyName, policy => policy.AddRequirements(new RoleRequirement(allowedRoles)));

        return services;
    }
}
