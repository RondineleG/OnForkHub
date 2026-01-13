namespace OnForkHub.CrossCutting.Storage;

using Microsoft.Extensions.Options;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Requests;
using System.IO;

using Path = System.IO.Path;

/// <summary>
/// Local file storage service implementation.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;
    private readonly string _uploadPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileStorageService"/> class.
    /// </summary>
    /// <param name="options">The file storage options.</param>
    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _options.BasePath, "videos");
        EnsureDirectoryExists();
    }

    /// <inheritdoc/>
    public async Task<RequestResult<string>> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        try
        {
            var fileId = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
            var filePath = Path.Combine(_uploadPath, fileId);

            using var fileStreamOutput = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true
            );
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

            var fileUrl = $"{_options.BaseUrl}/videos/{fileId}";
            return RequestResult<string>.Success(fileUrl);
        }
        catch (IOException ex)
        {
            return RequestResult<string>.WithError($"Failed to upload file: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RequestResult<string>.WithError($"Access denied when uploading file: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public Task<RequestResult> DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileId);

        try
        {
            var fileName = Path.GetFileName(fileId);
            var filePath = Path.Combine(_uploadPath, fileName);

            if (!File.Exists(filePath))
            {
                return Task.FromResult(RequestResult.WithError($"File not found: {fileId}"));
            }

            File.Delete(filePath);
            return Task.FromResult(RequestResult.Success());
        }
        catch (IOException ex)
        {
            return Task.FromResult(RequestResult.WithError($"Failed to delete file: {ex.Message}"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Task.FromResult(RequestResult.WithError($"Access denied when deleting file: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<RequestResult<Stream>> GetAsync(string fileId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileId);

        try
        {
            var fileName = Path.GetFileName(fileId);
            var filePath = Path.Combine(_uploadPath, fileName);

            if (!File.Exists(filePath))
            {
                return Task.FromResult(RequestResult<Stream>.WithError($"File not found: {fileId}"));
            }

            Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
            return Task.FromResult(RequestResult<Stream>.Success(fileStream));
        }
        catch (IOException ex)
        {
            return Task.FromResult(RequestResult<Stream>.WithError($"Failed to read file: {ex.Message}"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Task.FromResult(RequestResult<Stream>.WithError($"Access denied when reading file: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public RequestResult ValidateVideoFile(string fileName, string contentType, long fileSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!_options.AllowedVideoExtensions.Contains(extension))
        {
            return RequestResult.WithError($"Invalid file extension. Allowed extensions: {string.Join(", ", _options.AllowedVideoExtensions)}");
        }

        if (!string.IsNullOrWhiteSpace(contentType) && !_options.AllowedVideoContentTypes.Contains(contentType.ToLowerInvariant()))
        {
            return RequestResult.WithError($"Invalid content type. Allowed types: {string.Join(", ", _options.AllowedVideoContentTypes)}");
        }

        if (fileSize > _options.MaxFileSizeBytes)
        {
            var maxSizeMb = _options.MaxFileSizeBytes / (1024 * 1024);
            return RequestResult.WithError($"File size exceeds maximum allowed size of {maxSizeMb}MB");
        }

        if (fileSize <= 0)
        {
            return RequestResult.WithError("File is empty");
        }

        return RequestResult.Success();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }
}
