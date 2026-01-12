namespace OnForkHub.Application.Dtos.Video.Request;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for video upload requests.
/// </summary>
public sealed class VideoUploadRequestDto
{
    /// <summary>
    /// Gets or sets the video title.
    /// </summary>
    [Required(ErrorMessage = "The Title field is required")]
    [MinLength(3, ErrorMessage = "The Title field must be at least 3 characters long")]
    [MaxLength(50, ErrorMessage = "The Title field must be at most 50 characters long")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video description.
    /// </summary>
    [Required(ErrorMessage = "The Description field is required")]
    [MinLength(5, ErrorMessage = "The Description field must be at least 5 characters long")]
    [MaxLength(200, ErrorMessage = "The Description field must be at most 200 characters long")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID who owns the video.
    /// </summary>
    [Required(ErrorMessage = "The UserId field is required")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category IDs for the video.
    /// </summary>
    public IReadOnlyList<long> CategoryIds { get; set; } = [];
}
