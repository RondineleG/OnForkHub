namespace OnForkHub.CrossCutting.Storage;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.Extensions.Options;

using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Requests;

/// <summary>
/// Implementation of IFileStorageService using Azure Blob Storage.
/// </summary>
public sealed class AzureBlobStorageService : IFileStorageService
{
    private readonly AzureBlobStorageOptions _azureOptions;
    private readonly FileStorageOptions _fileOptions;
    private readonly BlobContainerClient _containerClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobStorageService"/> class.
    /// </summary>
    public AzureBlobStorageService(IOptions<AzureBlobStorageOptions> azureOptions, IOptions<FileStorageOptions> fileOptions)
    {
        _azureOptions = azureOptions.Value;
        _fileOptions = fileOptions.Value;

        if (string.IsNullOrEmpty(_azureOptions.ConnectionString))
        {
            // Fallback for development/testing if connection string is missing
            _containerClient = null!;
        }
        else
        {
            _containerClient = new BlobContainerClient(_azureOptions.ConnectionString, _azureOptions.ContainerName);
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<string>> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (_containerClient == null)
            {
                return RequestResult<string>.WithError("Azure Storage connection string is not configured.");
            }

            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

            var blobId = $"{Guid.NewGuid()}{System.IO.Path.GetExtension(fileName)}";
            var blobClient = _containerClient.GetBlobClient(blobId);

            var uploadOptions = new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } };

            await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);

            return RequestResult<string>.Success(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            return RequestResult<string>.WithError($"Failed to upload to Azure Blob Storage: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult> DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_containerClient == null)
            {
                return RequestResult.WithError("Azure Storage connection string is not configured.");
            }

            // Extract blob name from URL or ID
            var blobName = System.IO.Path.GetFileName(new Uri(fileId).LocalPath);
            var blobClient = _containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);

            return RequestResult.Success();
        }
        catch (Exception ex)
        {
            return RequestResult.WithError($"Failed to delete from Azure Blob Storage: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Stream>> GetAsync(string fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_containerClient == null)
            {
                return RequestResult<Stream>.WithError("Azure Storage connection string is not configured.");
            }

            var blobName = System.IO.Path.GetFileName(new Uri(fileId).LocalPath);
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                return RequestResult<Stream>.WithError("File not found in storage.");
            }

            var downloadInfo = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return RequestResult<Stream>.Success(downloadInfo.Value.Content);
        }
        catch (Exception ex)
        {
            return RequestResult<Stream>.WithError($"Failed to retrieve from Azure Blob Storage: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public RequestResult ValidateVideoFile(string fileName, string contentType, long fileSize)
    {
        if (fileSize > _fileOptions.MaxFileSizeBytes)
        {
            return RequestResult.WithError($"File size exceeds the limit of {_fileOptions.MaxFileSizeBytes / 1024 / 1024}MB");
        }

        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        if (!_fileOptions.AllowedVideoExtensions.Contains(extension))
        {
            return RequestResult.WithError($"File extension {extension} is not allowed.");
        }

        if (!_fileOptions.AllowedVideoContentTypes.Contains(contentType.ToLowerInvariant()))
        {
            return RequestResult.WithError($"Content type {contentType} is not allowed.");
        }

        return RequestResult.Success();
    }
}
