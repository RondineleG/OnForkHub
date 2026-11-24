namespace OnForkHub.Application.Dtos.Video.Request;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Data transfer object for video update requests.
/// </summary>
public sealed class VideoUpdateRequest
{
    /// <summary>
    /// Gets or sets the video ID.
    /// </summary>
    [Required(ErrorMessage = "The Id field is required")]
    public string Id { get; set; } = string.Empty;

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
    /// Gets or sets the video URL.
    /// </summary>
    [Required(ErrorMessage = "The Url field is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category IDs for the video.
    /// </summary>
    public IReadOnlyList<string> CategoryIds { get; set; } = [];

    /// <summary>
    /// Server-populated identifier of the authenticated requester. Ignored if supplied by the client.
    /// </summary>
    [JsonIgnore]
    public string? RequesterUserId { get; set; }

    /// <summary>
    /// Server-populated flag indicating whether the requester has the Admin role. Ignored if supplied by the client.
    /// </summary>
    [JsonIgnore]
    public bool RequesterIsAdmin { get; set; }
}
