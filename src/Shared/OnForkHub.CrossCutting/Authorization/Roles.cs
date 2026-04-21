namespace OnForkHub.CrossCutting.Authorization;

/// <summary>
/// Defines the application roles for authorization.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role with full access.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Moderator role with content management access.
    /// </summary>
    public const string Moderator = "Moderator";

    /// <summary>
    /// Regular user role with standard access.
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Premium user role with enhanced features.
    /// </summary>
    public const string Premium = "Premium";

    /// <summary>
    /// Gets all available roles.
    /// </summary>
    public static IReadOnlyList<string> All => [Admin, Moderator, User, Premium];

    /// <summary>
    /// Checks if the role is valid.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the role is valid, false otherwise.</returns>
    public static bool IsValid(string role)
    {
        return All.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
