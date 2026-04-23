namespace OnForkHub.Application.Services;

using Microsoft.Extensions.Logging;

using OnForkHub.Application.Services.Base;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service implementation for Video entity.
/// </summary>
public sealed class VideoService(IVideoRepositoryEF videoRepository, IFileStorageService fileStorageService, ILogger<VideoService> logger)
    : BaseService,
        IVideoService
{
    private readonly IVideoRepositoryEF _videoRepository = videoRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ILogger<VideoService> _logger = logger;

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> CreateAsync(Video video)
    {
        return await ExecuteAsync(async () => await _videoRepository.CreateAsync(video));
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> DeleteAsync(Id id)
    {
        return await ExecuteAsync(async () => await _videoRepository.DeleteAsync(id));
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Video>>> GetAllAsync(int page, int size)
    {
        return await ExecuteAsync(async () => await _videoRepository.GetAllAsync(page, size));
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> GetByIdAsync(Id id)
    {
        return await ExecuteAsync(async () => await _videoRepository.GetByIdAsync(id));
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Video>>> GetByUserIdAsync(Id userId, int page, int size)
    {
        return await ExecuteAsync(async () => await _videoRepository.GetByUserIdAsync(userId, page, size));
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Video>>> GetByCategoryIdAsync(long categoryId, int page, int size)
    {
        return await ExecuteAsync(async () => await _videoRepository.GetByCategoryIdAsync(categoryId, page, size));
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> UpdateAsync(Video video)
    {
        return await ExecuteAsync(async () => await _videoRepository.UpdateAsync(video));
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
                // Upload to storage
                var storageResult = await _fileStorageService.UploadAsync(fileStream, fileName, contentType, cancellationToken);

                if (storageResult.Status != EResultStatus.Success)
                {
                    return RequestResult<Video>.WithError(storageResult.Message ?? "Failed to upload file");
                }

                // Create database record
                var videoResult = Video.Create(title, description, storageResult.Data!, userId);
                if (videoResult.Status != EResultStatus.Success)
                {
                    return videoResult;
                }

                return await _videoRepository.CreateAsync(videoResult.Data!);
            },
            cancellationToken
        );
    }

    /// <inheritdoc/>
    public async Task<RequestResult> EnableTorrentAsync(Guid videoId, string magnetUri)
    {
        return await ExecuteAsync(async () =>
        {
            var videoResult = await _videoRepository.GetByIdAsync(videoId.ToString());
            if (videoResult.Status != EResultStatus.Success || videoResult.Data == null)
            {
                return RequestResult.WithError("Video not found");
            }

            videoResult.Data.EnableTorrent(magnetUri);
            await _videoRepository.UpdateAsync(videoResult.Data);
            return RequestResult.Success();
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult<(IEnumerable<Video> Items, int TotalCount)>> SearchAsync(
        string? searchTerm,
        long? categoryId,
        string? userId,
        DateTime? fromDate,
        DateTime? toDate,
        int sortBy,
        bool sortDescending,
        int page,
        int pageSize
    )
    {
        return await ExecuteAsync(
            async () => await _videoRepository.SearchAsync(searchTerm, categoryId, userId, fromDate, toDate, sortBy, sortDescending, page, pageSize),
            CancellationToken.None
        );
    }
}
