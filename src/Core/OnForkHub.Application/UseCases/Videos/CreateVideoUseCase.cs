namespace OnForkHub.Application.UseCases.Videos;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Core.ValueObjects;

/// <summary>
/// Use case for creating a video.
/// </summary>
public class CreateVideoUseCase(IVideoService videoService, IEntityValidator<Video> validator) : IUseCase<VideoCreateRequestDto, Video>
{
    private readonly IVideoService _videoService = videoService;
    private readonly IEntityValidator<Video> _validator = validator;

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> ExecuteAsync(VideoCreateRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var videoResult = request.ToVideo();
        if (videoResult.Status != EResultStatus.Success || videoResult.Data is null)
        {
            return RequestResult<Video>.WithError(videoResult.Message ?? "Failed to create video");
        }

        var validationResult = _validator.Validate(videoResult.Data);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new RequestValidation(e.Field, e.Message)).ToArray();
            return RequestResult<Video>.WithValidations(errors);
        }

        var result = await _videoService.CreateAsync(videoResult.Data);
        return result.Status != EResultStatus.Success || result.Data is null
            ? RequestResult<Video>.WithError(result.Message ?? "Failed to create video")
            : RequestResult<Video>.Success(result.Data);
    }
}
