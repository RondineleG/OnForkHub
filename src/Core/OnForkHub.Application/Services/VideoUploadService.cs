namespace OnForkHub.Application.Services;

using Microsoft.Extensions.Logging;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;

/// <summary>
/// Service implementation for managing video uploads.
/// </summary>
public class VideoUploadService(
    IVideoUploadRepository repository,
    IFileStorageService fileStorageService,
    ILogger<VideoUploadService> logger
) : BaseService, IVideoUploadService
{
    private const long MaxFileSize = 100 * 1024 * 1024; // 100MB

    private readonly IVideoUploadRepository _repository = repository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ILogger<VideoUploadService> _logger = logger;

    /// <inheritdoc/>
    public async Task<RequestResult<VideoUploadResponse>> InitiateUploadAsync(
        string fileName,
        long fileSize,
        string contentType,
        string userId
    )
    {
        return await ExecuteAsync(async () =>
        {
            if (fileSize > MaxFileSize)
            {
                return RequestResult<VideoUploadResponse>.WithError($"File size exceeds the maximum limit of {MaxFileSize / 1024 / 1024}MB");
            }

            var uploadResult = VideoUpload.Create(fileName, fileSize, contentType, userId);
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
    public async Task<RequestResult<bool>> UploadChunkAsync(
        Guid uploadId,
        Stream chunk,
        int chunkIndex,
        int totalChunks
    )
    {
        return await ExecuteAsync(async () =>
        {
            var uploadResult = await _repository.GetByIdAsync(uploadId);
            if (!uploadResult.Status.Equals(EResultStatus.Success) || uploadResult.Data is null)
            {
                return RequestResult<bool>.WithError(uploadResult.Message ?? "Upload not found");
            }

            var upload = uploadResult.Data;

            // TODO: In a real implementation, we would save the chunk to temporary storage
            // For now, we just update the progress
            upload.IncrementReceivedChunks();

            if (upload.ReceivedChunks == upload.TotalChunks)
            {
                // Last chunk received, mark as processing
                // In a real implementation, we would assemble the file and upload to final storage
                upload.MarkAsProcessing();
            }

            await _repository.UpdateAsync(upload);
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
    public async Task<RequestResult<IReadOnlyList<VideoUploadResponse>>> GetUserUploadsAsync(
        string userId,
        int page = 1,
        int pageSize = 20
    )
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
