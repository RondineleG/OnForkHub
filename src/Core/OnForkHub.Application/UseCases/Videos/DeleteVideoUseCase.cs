namespace OnForkHub.Application.UseCases.Videos;

using OnForkHub.Core.ValueObjects;

/// <summary>
/// Use case for deleting a video.
/// </summary>
public class DeleteVideoUseCase(IVideoService videoService) : IUseCase<string, Video>
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
        var result = await _videoService.DeleteAsync(id);

        return result.Status != EResultStatus.Success
            ? RequestResult<Video>.WithError(result.Message ?? $"Failed to delete video with ID {request}")
            : RequestResult<Video>.Success(result.Data!);
    }
}
