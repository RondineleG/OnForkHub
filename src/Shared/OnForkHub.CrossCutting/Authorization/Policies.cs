namespace OnForkHub.CrossCutting.Authorization;

/// <summary>
/// Defines the authorization policies for the application.
/// </summary>
public static class Policies
{
    /// <summary>
    /// Policy requiring admin role.
    /// </summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>
    /// Policy requiring moderator or higher role.
    /// </summary>
    public const string RequireModerator = "RequireModerator";

    /// <summary>
    /// Policy requiring authenticated user.
    /// </summary>
    public const string RequireUser = "RequireUser";

    /// <summary>
    /// Policy requiring premium subscription.
    /// </summary>
    public const string RequirePremium = "RequirePremium";

    /// <summary>
    /// Policy for content management operations.
    /// </summary>
    public const string CanManageContent = "CanManageContent";

    /// <summary>
    /// Policy for user management operations.
    /// </summary>
    public const string CanManageUsers = "CanManageUsers";

    /// <summary>
    /// Policy for video upload operations.
    /// </summary>
    public const string CanUploadVideos = "CanUploadVideos";

    /// <summary>
    /// Policy for viewing premium content.
    /// </summary>
    public const string CanViewPremiumContent = "CanViewPremiumContent";
}
