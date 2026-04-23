namespace OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service interface for file storage operations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file and returns the URL.
    /// </summary>
    /// <param name="fileStream">The file stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The URL of the uploaded file.</returns>
    Task<RequestResult<string>> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file by its identifier.
    /// </summary>
    /// <param name="fileId">The file identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<RequestResult> DeleteAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a file stream by its identifier.
    /// </summary>
    /// <param name="fileId">The file identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The file stream.</returns>
    Task<RequestResult<Stream>> GetAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a video file.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="fileSize">The file size in bytes.</param>
    /// <returns>A validation result.</returns>
    RequestResult ValidateVideoFile(string fileName, string contentType, long fileSize);
}
