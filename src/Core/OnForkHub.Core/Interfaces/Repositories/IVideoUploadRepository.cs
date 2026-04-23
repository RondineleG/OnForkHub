namespace OnForkHub.Core.Interfaces.Repositories;

using OnForkHub.Core.Entities;

/// <summary>
/// Repository interface for VideoUpload entity.
/// </summary>
public interface IVideoUploadRepository
{
    /// <summary>
    /// Adds a new video upload.
    /// </summary>
    /// <param name="upload">The video upload entity.</param>
    /// <returns>The created video upload.</returns>
    Task<RequestResult<VideoUpload>> AddAsync(VideoUpload upload);

    /// <summary>
    /// Gets a video upload by identifier.
    /// </summary>
    /// <param name="id">The upload identifier.</param>
    /// <returns>The video upload entity.</returns>
    Task<RequestResult<VideoUpload>> GetByIdAsync(Guid id);

    /// <summary>
    /// Updates an existing video upload.
    /// </summary>
    /// <param name="upload">The video upload entity.</param>
    /// <returns>The updated video upload.</returns>
    Task<RequestResult<VideoUpload>> UpdateAsync(VideoUpload upload);

    /// <summary>
    /// Gets video uploads for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A list of video uploads.</returns>
    Task<RequestResult<IEnumerable<VideoUpload>>> GetByUserIdAsync(string userId, int page, int pageSize);

    /// <summary>
    /// Gets the total count of uploads for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The total count.</returns>
    Task<int> GetCountByUserIdAsync(string userId);
}
