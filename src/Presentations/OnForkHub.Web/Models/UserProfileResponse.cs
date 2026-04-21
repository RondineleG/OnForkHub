namespace OnForkHub.Web.Models;

/// <summary>
/// Response DTO for user profile data.
/// </summary>
public class UserProfileResponse
{
    /// <summary>Gets or sets the user unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the user display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the user email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the avatar URL.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the account creation date.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the number of videos uploaded by this user.</summary>
    public int VideoCount { get; set; }
}
