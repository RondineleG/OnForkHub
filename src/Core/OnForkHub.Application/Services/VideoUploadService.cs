namespace OnForkHub.Application.Services;

using System.Globalization;
using System.IO;

using Microsoft.Extensions.Logging;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;

/// <summary>
/// Service implementation for managing video uploads.
/// </summary>
public class VideoUploadService(IVideoUploadRepository repository, IFileStorageService fileStorageService, ILogger<VideoUploadService> logger)
    : BaseService,
        IVideoUploadService
{
    private const long MaxFileSize = 100 * 1024 * 1024; // 100MB

    private readonly IVideoUploadRepository _repository = repository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ILogger<VideoUploadService> _logger = logger;

    /// <inheritdoc/>
    public async Task<RequestResult<VideoUploadResponse>> InitiateUploadAsync(string fileName, long fileSize, string contentType, string userId, int totalChunks)
    {
        return await ExecuteAsync(async () =>
        {
            if (fileSize <= 0 || fileSize > MaxFileSize || totalChunks < 1 || totalChunks > 2048)
            {
                return RequestResult<VideoUploadResponse>.WithError($"File size exceeds the maximum limit of {MaxFileSize / 1024 / 1024}MB");
            }

            var uploadResult = VideoUpload.Create(fileName, fileSize, contentType, userId, totalChunks);
            if (!uploadResult.Status.Equals(EResultStatus.Success) || uploadResult.Data is null)
            {
                return RequestResult<VideoUploadResponse>.WithError(uploadResult.Message ?? "Failed to create upload entity");
            }

            var addResult = await _repository.AddAsync(uploadResult.Data);
            if (!addResult.Status.Equals(EResultStatus.Success) || addResult.Data is null)
            {
                return RequestResult<VideoUploadResponse>.WithError(addResult.Message ?? "Failed to save upload entity");
            }

            return RequestResult<VideoUploadResponse>.Success(VideoUploadResponse.FromEntity(addResult.Data));
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult<bool>> UploadChunkAsync(Guid uploadId, Stream chunk, int chunkIndex, int totalChunks)
    {
        return await ExecuteAsync(async () =>
        {
            var uploadResult = await _repository.GetByIdAsync(uploadId);
            if (!uploadResult.Status.Equals(EResultStatus.Success) || uploadResult.Data is null)
            {
                return RequestResult<bool>.WithError(uploadResult.Message ?? "Upload not found");
            }

            var upload = uploadResult.Data;

            if (totalChunks != upload.TotalChunks || chunkIndex != upload.ReceivedChunks ||
                chunkIndex < 0 || chunkIndex >= totalChunks ||
                (upload.Status != EVideoUploadStatus.Pending && upload.Status != EVideoUploadStatus.Uploading))
            {
                return RequestResult<bool>.WithError("Invalid upload chunk sequence or state.");
            }

            // A single-node staging implementation. Multi-node deployments require shared storage
            // and per-upload coordination before chunk processing can be scaled horizontally.
            var stagingRoot = Path.Combine(Path.GetTempPath(), "onforkhub-uploads");
            var uploadDirectory = Path.Combine(stagingRoot, uploadId.ToString("N"));
            Directory.CreateDirectory(uploadDirectory);
            var chunkPath = Path.Combine(uploadDirectory, chunkIndex.ToString("D6", CultureInfo.InvariantCulture) + ".part");
            if (File.Exists(chunkPath))
            {
                return RequestResult<bool>.WithError("Chunk already exists.");
            }

            await using (var destination = new FileStream(chunkPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await chunk.CopyToAsync(destination);
            }
            if (new FileInfo(chunkPath).Length == 0 ||
                new FileInfo(chunkPath).Length > upload.FileSize)
            {
                File.Delete(chunkPath);
                return RequestResult<bool>.WithError("Chunk size exceeds allowed upload size.");
            }

            if (upload.Status == EVideoUploadStatus.Pending)
            {
                upload.MarkAsUploading();
            }

            upload.IncrementReceivedChunks();

            if (upload.ReceivedChunks == upload.TotalChunks)
            {
                upload.MarkAsProcessing();
                var assembledPath = Path.Combine(uploadDirectory, "assembled.video");
                await using (var assembled = new FileStream(assembledPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    for (var index = 0; index < totalChunks; index++)
                    {
                        var partPath = Path.Combine(uploadDirectory, index.ToString("D6", CultureInfo.InvariantCulture) + ".part");
                        await using var part = new FileStream(partPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        await part.CopyToAsync(assembled);
                    }
                }

                var actualSize = new FileInfo(assembledPath).Length;
                if (actualSize != upload.FileSize)
                {
                    upload.MarkAsFailed("Upload size mismatch.");
                    await _repository.UpdateAsync(upload);
                    return RequestResult<bool>.WithError("Upload size mismatch.");
                }

                await using var source = new FileStream(assembledPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var stored = await _fileStorageService.UploadAsync(source, upload.FileName, upload.ContentType);
                if (stored.Status != EResultStatus.Success || string.IsNullOrWhiteSpace(stored.Data))
                {
                    upload.MarkAsFailed(stored.Message ?? "Unable to persist uploaded file.");
                    await _repository.UpdateAsync(upload);
                    return RequestResult<bool>.WithError(stored.Message ?? "Unable to persist uploaded file.");
                }

                upload.MarkAsCompleted(stored.Data);
            }

            var updateResult = await _repository.UpdateAsync(upload);
            if (updateResult.Status != EResultStatus.Success)
            {
                return RequestResult<bool>.WithError(updateResult.Message ?? "Unable to save upload progress.");
            }
            if (upload.Status == EVideoUploadStatus.Completed)
            {
                try
                {
                    Directory.Delete(uploadDirectory, recursive: true);
                }
                catch (IOException)
                {
                    // Cleanup can be retried by maintenance.
                }
                catch (UnauthorizedAccessException)
                {
                    // Do not undo completed upload.
                }
            }
            return RequestResult<bool>.Success(true);
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult<EVideoUploadStatus>> GetUploadStatusAsync(Guid uploadId)
    {
        return await ExecuteAsync(async () =>
        {
            var uploadResult = await _repository.GetByIdAsync(uploadId);
            if (!uploadResult.Status.Equals(EResultStatus.Success) || uploadResult.Data is null)
            {
                return RequestResult<EVideoUploadStatus>.WithError(uploadResult.Message ?? "Upload not found");
            }

            return RequestResult<EVideoUploadStatus>.Success(uploadResult.Data.Status);
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IReadOnlyList<VideoUploadResponse>>> GetUserUploadsAsync(string userId, int page = 1, int pageSize = 20)
    {
        return await ExecuteAsync(async () =>
        {
            var uploadsResult = await _repository.GetByUserIdAsync(userId, page, pageSize);
            if (!uploadsResult.Status.Equals(EResultStatus.Success) || uploadsResult.Data is null)
            {
                return RequestResult<IReadOnlyList<VideoUploadResponse>>.WithError(uploadsResult.Message ?? "Failed to retrieve uploads");
            }

            var responses = uploadsResult.Data.Select(VideoUploadResponse.FromEntity).ToList();
            return RequestResult<IReadOnlyList<VideoUploadResponse>>.Success(responses);
        });
    }
}
