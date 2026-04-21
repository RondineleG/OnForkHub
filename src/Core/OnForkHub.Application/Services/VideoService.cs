namespace OnForkHub.Application.Services;

/// <summary>
/// Service implementation for Video operations.
/// </summary>
public class VideoService(IVideoRepositoryEF videoRepository, IFileStorageService fileStorageService, IValidationService<Video> validationService)
    : BaseService,
        IVideoService
{
    private readonly IVideoRepositoryEF _videoRepository = videoRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IValidationService<Video> _validationService = validationService;

    /// <inheritdoc/>
    public Task<RequestResult<Video>> CreateAsync(Video video)
    {
        return ExecuteAsync(video, _videoRepository.CreateAsync, _validationService);
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> DeleteAsync(Id id)
    {
        return await ExecuteAsync(async () =>
        {
            var videoResult = await _videoRepository.GetByIdAsync(id);

            if (!videoResult.Status.Equals(EResultStatus.Success) || videoResult.Data is null)
            {
                return videoResult;
            }

            var video = videoResult.Data;

            if (!string.IsNullOrEmpty(video.Url?.Value))
            {
                await _fileStorageService.DeleteAsync(video.Url.Value);
            }

            return await _videoRepository.DeleteAsync(id);
        });
    }

    /// <inheritdoc/>
    public Task<RequestResult<IEnumerable<Video>>> GetAllAsync(int page, int size)
    {
        return ExecuteAsync(async () => await _videoRepository.GetAllAsync(page, size));
    }

    /// <inheritdoc/>
    public Task<RequestResult<Video>> GetByIdAsync(Id id)
    {
        return ExecuteAsync(async () =>
        {
            var result = await _videoRepository.GetByIdAsync(id);
            return !result.Status.Equals(EResultStatus.Success) ? RequestResult<Video>.WithError($"Video with id {id} not found") : result;
        });
    }

    /// <inheritdoc/>
    public Task<RequestResult<IEnumerable<Video>>> GetByUserIdAsync(Id userId, int page, int size)
    {
        return ExecuteAsync(async () => await _videoRepository.GetByUserIdAsync(userId, page, size));
    }

    /// <inheritdoc/>
    public Task<RequestResult<IEnumerable<Video>>> GetByCategoryIdAsync(long categoryId, int page, int size)
    {
        return ExecuteAsync(async () => await _videoRepository.GetByCategoryIdAsync(categoryId, page, size));
    }

    /// <inheritdoc/>
    public Task<RequestResult<Video>> UpdateAsync(Video video)
    {
        return ExecuteAsync(video, _videoRepository.UpdateAsync, _validationService);
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string title,
        string description,
        Id userId,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteAsync(
            async () =>
            {
                var validationResult = _fileStorageService.ValidateVideoFile(fileName, contentType, fileStream.Length);
                if (!validationResult.Status.Equals(EResultStatus.Success))
                {
                    return RequestResult<Video>.WithError(validationResult.Message ?? "Invalid video file");
                }

                var uploadResult = await _fileStorageService.UploadAsync(fileStream, fileName, contentType, cancellationToken);
                if (!uploadResult.Status.Equals(EResultStatus.Success) || uploadResult.Data is null)
                {
                    return RequestResult<Video>.WithError(uploadResult.Message ?? "Failed to upload video file");
                }

                var videoUrl = uploadResult.Data;
                var videoResult = Video.Create(title, description, videoUrl, userId);

                if (!videoResult.Status.Equals(EResultStatus.Success) || videoResult.Data is null)
                {
                    await _fileStorageService.DeleteAsync(videoUrl, cancellationToken);
                    return videoResult;
                }

                var createResult = await _videoRepository.CreateAsync(videoResult.Data);

                if (!createResult.Status.Equals(EResultStatus.Success))
                {
                    await _fileStorageService.DeleteAsync(videoUrl, cancellationToken);
                    return createResult;
                }

                return createResult;
            },
            cancellationToken
        );
    }
}
