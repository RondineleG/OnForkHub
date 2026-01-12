namespace OnForkHub.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Video entity using Entity Framework.
/// </summary>
public interface IVideoRepositoryEF
{
    /// <summary>
    /// Creates a new video.
    /// </summary>
    /// <param name="video">The video to create.</param>
    /// <returns>The created video.</returns>
    Task<RequestResult<Video>> CreateAsync(Video video);

    /// <summary>
    /// Deletes a video by ID.
    /// </summary>
    /// <param name="id">The video ID.</param>
    /// <returns>The deleted video.</returns>
    Task<RequestResult<Video>> DeleteAsync(Id id);

    /// <summary>
    /// Gets all videos with pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="size">The page size.</param>
    /// <returns>A list of videos.</returns>
    Task<RequestResult<IEnumerable<Video>>> GetAllAsync(int page, int size);

    /// <summary>
    /// Gets a video by ID.
    /// </summary>
    /// <param name="id">The video ID.</param>
    /// <returns>The video.</returns>
    Task<RequestResult<Video>> GetByIdAsync(Id id);

    /// <summary>
    /// Gets videos by user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="page">The page number.</param>
    /// <param name="size">The page size.</param>
    /// <returns>A list of videos.</returns>
    Task<RequestResult<IEnumerable<Video>>> GetByUserIdAsync(Id userId, int page, int size);

    /// <summary>
    /// Gets videos by category ID.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="page">The page number.</param>
    /// <param name="size">The page size.</param>
    /// <returns>A list of videos.</returns>
    Task<RequestResult<IEnumerable<Video>>> GetByCategoryIdAsync(long categoryId, int page, int size);

    /// <summary>
    /// Updates a video.
    /// </summary>
    /// <param name="video">The video to update.</param>
    /// <returns>The updated video.</returns>
    Task<RequestResult<Video>> UpdateAsync(Video video);
}
