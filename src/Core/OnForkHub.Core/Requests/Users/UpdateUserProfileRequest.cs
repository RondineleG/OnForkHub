namespace OnForkHub.Core.Requests.Users;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request for updating a user's profile.
/// </summary>
public sealed class UpdateUserProfileRequest
{
    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    [Required(ErrorMessage = "The Name field is required")]
    [MaxLength(100, ErrorMessage = "The Name field must be at most 100 characters long")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email.
    /// </summary>
    [Required(ErrorMessage = "The Email field is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the user's preferences for autoplay.
    /// </summary>
    public bool? AutoPlayNextVideo { get; set; }

    /// <summary>
    /// Gets or sets the user's default video quality.
    /// </summary>
    public string? DefaultQuality { get; set; }

    /// <summary>
    /// Gets or sets whether P2P is enabled for the user.
    /// </summary>
    public bool? EnableP2P { get; set; }

    /// <summary>
    /// Gets or sets the P2P download limit.
    /// </summary>
    public double? DownloadLimitMb { get; set; }

    /// <summary>
    /// Gets or sets the P2P upload limit.
    /// </summary>
    public double? UploadLimitMb { get; set; }

    /// <summary>
    /// Gets or sets whether dark mode is enabled.
    /// </summary>
    public bool? DarkMode { get; set; }

    /// <summary>
    /// Gets or sets the preferred language.
    /// </summary>
    public string? Language { get; set; }
}
