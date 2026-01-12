namespace OnForkHub.Application.UseCases.Videos;

/// <summary>
/// Use case for getting all videos with pagination.
/// </summary>
public class GetAllVideoUseCase(IVideoService videoService) : IUseCase<PaginationRequestDto, IEnumerable<Video>>
{
    private readonly IVideoService _videoService = videoService;

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Video>>> ExecuteAsync(PaginationRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _videoService.GetAllAsync(request.Page, request.ItemsPerPage);
        return result?.Data is null ? RequestResult<IEnumerable<Video>>.WithNoContent() : RequestResult<IEnumerable<Video>>.Success(result.Data);
    }
}
