namespace OnForkHub.Core.Responses;

using OnForkHub.Core.Entities;

/// <summary>
/// Data transfer object for video responses.
/// </summary>
public sealed class VideoResponse
{
    /// <summary>
    /// Gets or sets the video identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the thumbnail URL.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds.
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Gets or sets the number of views.
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// Gets or sets the owner's identifier.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner's name.
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether P2P streaming is enabled.
    /// </summary>
    public bool IsTorrentEnabled { get; set; }

    /// <summary>
    /// Creates a VideoResponse from a Video entity.
    /// </summary>
    /// <param name="video">The video entity.</param>
    /// <returns>The video response.</returns>
    public static VideoResponse FromVideo(Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        return new VideoResponse
        {
            Id = video.Id.ToString(),
            Title = video.Title.Value,
            Description = video.Description,
            Url = video.Url.Value,
            OwnerId = video.UserId?.ToString() ?? string.Empty,
            CreatedAt = video.CreatedAt,
            IsTorrentEnabled = video.IsTorrentEnabled,
        };
    }
}
