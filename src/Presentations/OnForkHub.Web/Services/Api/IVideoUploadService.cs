namespace OnForkHub.Web.Services.Api;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Responses;

/// <summary>
/// Service contract for video upload API operations.
/// </summary>
public interface IVideoUploadService
{
    /// <summary>
    /// Initiates a new chunked upload.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<VideoUploadResponse> InitiateUploadAsync(string fileName, long fileSize, string contentType);

    /// <summary>
    /// Uploads a chunk of the video file.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<bool> UploadChunkAsync(Guid uploadId, byte[] chunk, int chunkIndex, int totalChunks);

    /// <summary>
    /// Gets the current status of an upload.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<string> GetUploadStatusAsync(Guid uploadId);

    /// <summary>
    /// Gets the list of uploads for the current user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<List<VideoUploadResponse>> GetMyUploadsAsync(int page = 1, int pageSize = 20);
}
