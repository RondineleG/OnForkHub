namespace OnForkHub.Application.UseCases.Videos;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Core.ValueObjects;

/// <summary>
/// Use case for updating a video.
/// </summary>
public class UpdateVideoUseCase(IVideoService videoService, IEntityValidator<Video> validator) : IUseCase<VideoUpdateRequestDto, Video>
{
    private readonly IVideoService _videoService = videoService;
    private readonly IEntityValidator<Video> _validator = validator;

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> ExecuteAsync(VideoUpdateRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

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

        var video = existingResult.Data;
        var updateResult = video.UpdateVideo(request.Title, request.Description, request.Url);

        if (updateResult.Status != EResultStatus.Success)
        {
            return RequestResult<Video>.WithError(updateResult.Message ?? "Failed to update video");
        }

        var validationResult = _validator.ValidateUpdate(video);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new RequestValidation(e.Field, e.Message)).ToArray();
            return RequestResult<Video>.WithValidations(errors);
        }

        var result = await _videoService.UpdateAsync(video);
        return result.Status != EResultStatus.Success || result.Data is null
            ? RequestResult<Video>.WithError(result.Message ?? "Failed to update video")
            : RequestResult<Video>.Success(result.Data);
    }
}
