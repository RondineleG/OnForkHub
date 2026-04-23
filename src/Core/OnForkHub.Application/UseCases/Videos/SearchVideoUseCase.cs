namespace OnForkHub.Application.UseCases.Videos;

using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Responses;

/// <summary>
/// Use case for searching videos with filters.
/// </summary>
public class SearchVideoUseCase(IVideoRepositoryEF repository) : IUseCase<VideoSearchRequestDto, PagedResultDto<VideoResponse>>
{
    private readonly IVideoRepositoryEF _repository = repository;

    /// <inheritdoc/>
    public async Task<RequestResult<PagedResultDto<VideoResponse>>> ExecuteAsync(VideoSearchRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _repository.SearchAsync(
            request.SearchTerm,
            request.CategoryId,
            request.UserId,
            request.FromDate,
            request.ToDate,
            (int)request.SortBy,
            request.SortDescending,
            request.Page,
            request.ItemsPerPage
        );

        if (result.Status != EResultStatus.Success || result.Data.Items is null)
        {
            return RequestResult<PagedResultDto<VideoResponse>>.WithError(result.Message ?? "Failed to search videos");
        }

        var responseDtos = result.Data.Items.Select(VideoResponse.FromVideo).ToList();

        var pagedResult = PagedResultDto<VideoResponse>.Create(responseDtos, request.Page, request.ItemsPerPage, result.Data.TotalCount);

        return RequestResult<PagedResultDto<VideoResponse>>.Success(pagedResult);
    }
}
