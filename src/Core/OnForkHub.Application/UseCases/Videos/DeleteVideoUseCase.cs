namespace OnForkHub.Application.UseCases.Videos;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Core.ValueObjects;

/// <summary>
/// Use case for deleting a video. Only the owner or an administrator can delete a video.
/// </summary>
public class DeleteVideoUseCase(IVideoService videoService) : IUseCase<VideoDeleteRequest, Video>
{
    private readonly IVideoService _videoService = videoService;

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> ExecuteAsync(VideoDeleteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Id);

        if (!Guid.TryParse(request.Id, out _))
        {
            return RequestResult<Video>.WithError("Invalid video ID format");
        }

        Id id = request.Id;
        var existingResult = await _videoService.GetByIdAsync(id);
        if (existingResult.Status != EResultStatus.Success || existingResult.Data is null)
        {
            return RequestResult<Video>.WithError(existingResult.Message ?? $"Video with ID {request.Id} not found");
        }

        if (!UpdateVideoUseCase.IsOwnerOrAdmin(existingResult.Data, request.RequesterUserId, request.RequesterIsAdmin))
        {
            return new RequestResult<Video> { Status = EResultStatus.Forbidden, Message = "You do not have permission to delete this video" };
        }

        var result = await _videoService.DeleteAsync(id);

        return result.Status != EResultStatus.Success
            ? RequestResult<Video>.WithError(result.Message ?? $"Failed to delete video with ID {request.Id}")
            : RequestResult<Video>.Success(result.Data!);
    }
}
