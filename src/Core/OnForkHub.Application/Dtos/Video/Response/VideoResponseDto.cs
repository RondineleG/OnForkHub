namespace OnForkHub.Application.Dtos.Video.Response;

/// <summary>
/// Data transfer object for video responses.
/// </summary>
public sealed class VideoResponseDto
{
    /// <summary>
    /// Gets or sets the video ID.
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
    /// Gets or sets the user ID who owns the video.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category names for the video.
    /// </summary>
    public IReadOnlyList<string> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Creates a VideoResponseDto from a Video entity.
    /// </summary>
    /// <param name="video">The video entity.</param>
    /// <returns>The video response DTO.</returns>
    public static VideoResponseDto FromVideo(Core.Entities.Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        return new VideoResponseDto
        {
            Id = video.Id.ToString(),
            Title = video.Title.Value,
            Description = video.Description,
            Url = video.Url.Value,
            UserId = video.UserId?.ToString() ?? string.Empty,
            Categories = video.Categories.Select(c => c.Name.Value).ToList(),
            CreatedAt = video.CreatedAt,
            UpdatedAt = video.UpdatedAt,
        };
    }
}
