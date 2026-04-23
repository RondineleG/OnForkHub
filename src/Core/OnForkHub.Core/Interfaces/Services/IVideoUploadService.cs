namespace OnForkHub.Core.Interfaces.Services;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Responses;

/// <summary>
/// Service for managing video uploads.
/// </summary>
public interface IVideoUploadService
{
    /// <summary>
    /// Initiates a new video upload.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="fileSize">The total file size.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The initiated upload response.</returns>
    Task<RequestResult<VideoUploadResponse>> InitiateUploadAsync(string fileName, long fileSize, string contentType, string userId);

    /// <summary>
    /// Uploads a chunk of the video file.
    /// </summary>
    /// <param name="uploadId">The upload identifier.</param>
    /// <param name="chunk">The chunk stream.</param>
    /// <param name="chunkIndex">The current chunk index.</param>
    /// <param name="totalChunks">The total number of chunks.</param>
    /// <returns>True if the chunk was uploaded successfully.</returns>
    Task<RequestResult<bool>> UploadChunkAsync(Guid uploadId, Stream chunk, int chunkIndex, int totalChunks);

    /// <summary>
    /// Gets the current status of an upload.
    /// </summary>
    /// <param name="uploadId">The upload identifier.</param>
    /// <returns>The upload status.</returns>
    Task<RequestResult<EVideoUploadStatus>> GetUploadStatusAsync(Guid uploadId);

    /// <summary>
    /// Gets the uploads for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A list of video upload responses.</returns>
    Task<RequestResult<IReadOnlyList<VideoUploadResponse>>> GetUserUploadsAsync(string userId, int page = 1, int pageSize = 20);
}
