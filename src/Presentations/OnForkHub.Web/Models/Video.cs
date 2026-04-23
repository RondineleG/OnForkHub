namespace OnForkHub.Web.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// UI model for Video entity.
/// </summary>
public class Video
{
    /// <summary>
    /// Gets or sets the video identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the video name (alias for Title for backward compatibility).
    /// </summary>
    public string? Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video title.
    /// </summary>
    [Required]
    public string? Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video description.
    /// </summary>
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video URL.
    /// </summary>
    public string? Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the thumbnail URL.
    /// </summary>
    public string? Thumbnail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration in seconds.
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of views.
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the video is available via P2P.
    /// </summary>
    public bool IsTorrent { get; set; }
}
