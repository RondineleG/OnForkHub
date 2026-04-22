namespace OnForkHub.Web.Services.Api;

using OnForkHub.Core.Responses;
using OnForkHub.Web.Models;

/// <summary>
/// Service contract for video API operations.
/// </summary>
public interface IVideoService
{
    /// <summary>
    /// Gets all videos with filtering and pagination.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<List<Video>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null, long? categoryId = null, string? sort = null);

    /// <summary>
    /// Gets a video by identifier.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<Video?> GetByIdAsync(string id);

    /// <summary>
    /// Searches for videos with advanced filters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<SearchResponse<Video>> SearchAsync(string? q, long? categoryId = null, int page = 1, int pageSize = 20);

    // Compatibility methods for existing UI components
    Task<List<Video>> GetVideosAsync(int page = 1, int pageSize = 10);
    Task<Video?> GetVideoByIdAsync(string id);
    Task<List<Video>> GetRelatedVideosAsync(string videoId, int count = 5);
    Task<bool> LikeVideoAsync(string id);
    Task<bool> UnlikeVideoAsync(string id);
    Task<bool> DeleteVideoAsync(string id);
    Task<bool> UploadVideoAsync(Stream fileStream, string fileName, string title, string description, string categoryId);
}
