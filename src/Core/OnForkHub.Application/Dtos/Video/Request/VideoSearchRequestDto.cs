namespace OnForkHub.Application.Dtos.Video.Request;

/// <summary>
/// Data transfer object for video search requests.
/// </summary>
public sealed class VideoSearchRequestDto : PaginationRequestDto
{
    /// <summary>
    /// Gets or sets the search term for title and description.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the category ID filter.
    /// </summary>
    public long? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the user ID filter.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the minimum creation date filter.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Gets or sets the maximum creation date filter.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    public VideoSortField SortBy { get; set; } = VideoSortField.CreatedAt;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the sort direction.
    /// </summary>
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Available fields for sorting videos.
/// </summary>
public enum VideoSortField
{
    /// <summary>
    /// Sort by creation date.
    /// </summary>
    CreatedAt,

    /// <summary>
    /// Sort by title.
    /// </summary>
    Title,

    /// <summary>
    /// Sort by update date.
    /// </summary>
    UpdatedAt,
}
