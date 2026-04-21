namespace OnForkHub.Web.Models;

/// <summary>
/// Represents a video DTO returned from the API.
/// </summary>
public class VideoDto
{
    /// <summary>Gets or sets the video unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the video title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the video description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the thumbnail URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the category name.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Gets or sets the category ID.</summary>
    public long CategoryId { get; set; }

    /// <summary>Gets or sets the view count.</summary>
    public int ViewCount { get; set; }

    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the uploader name.</summary>
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the uploader ID.</summary>
    public string UploadedById { get; set; } = string.Empty;

    /// <summary>Gets or sets the video URL (for playback).</summary>
    public string? VideoUrl { get; set; }

    /// <summary>Gets or sets the duration in seconds.</summary>
    public int? DurationSeconds { get; set; }

    /// <summary>Gets or sets the like count.</summary>
    public int LikeCount { get; set; }

    /// <summary>Gets or sets a value indicating whether gets or sets whether the current user has liked this video.</summary>
    public bool IsLikedByCurrentUser { get; set; }

    /// <summary>Gets or sets the tags associated with the video.</summary>
    public List<string> Tags { get; set; } = new();
}
