namespace OnForkHub.Web.Services.Api;

using OnForkHub.Web.Models;

/// <summary>
/// Service contract for video API operations.
/// </summary>
public interface IVideoService
{
    /// <summary>
    /// Gets a paginated list of videos with optional filters.
    /// </summary>
    Task<PagedResult<VideoDto>> GetVideosAsync(
        int page = 1,
        int pageSize = 12,
        string? search = null,
        long? categoryId = null,
        string? sort = null);

    /// <summary>
    /// Gets a single video by its ID.
    /// </summary>
    Task<VideoDto?> GetVideoByIdAsync(string id);

    /// <summary>
    /// Gets related videos for a given video ID.
    /// </summary>
    Task<List<VideoDto>> GetRelatedVideosAsync(string videoId, int count = 6);

    /// <summary>
    /// Uploads a new video with metadata.
    /// </summary>
    Task<VideoDto> UploadVideoAsync(
        Stream fileStream,
        string fileName,
        string title,
        string description,
        long categoryId,
        string tags,
        IProgress<double>? progress = null);

    /// <summary>
    /// Deletes a video by its ID.
    /// </summary>
    Task DeleteVideoAsync(string id);

    /// <summary>
    /// Likes a video.
    /// </summary>
    Task LikeVideoAsync(string id);

    /// <summary>
    /// Unlikes a video.
    /// </summary>
    Task UnlikeVideoAsync(string id);
}
