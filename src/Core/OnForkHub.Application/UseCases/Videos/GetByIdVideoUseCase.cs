namespace OnForkHub.Application.UseCases.Videos;

using OnForkHub.Core.ValueObjects;

/// <summary>
/// Use case for getting a video by ID.
/// </summary>
public class GetByIdVideoUseCase(IVideoService videoService) : IUseCase<string, Video>
{
    private readonly IVideoService _videoService = videoService;

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> ExecuteAsync(string request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request);

        if (!Guid.TryParse(request, out _))
        {
            return RequestResult<Video>.WithError("Invalid video ID format");
        }

        Id id = request;
        var result = await _videoService.GetByIdAsync(id);

        return result.Status != EResultStatus.Success || result.Data is null
            ? RequestResult<Video>.WithError(result.Message ?? $"Video with ID {request} not found")
            : RequestResult<Video>.Success(result.Data);
    }
}
