namespace OnForkHub.Application.UseCases.Videos;

using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Application.Dtos.Video.Response;
using OnForkHub.Core.Interfaces.Repositories;

/// <summary>
/// Use case for searching videos with filters.
/// </summary>
public class SearchVideoUseCase(IVideoRepositoryEF repository) : IUseCase<VideoSearchRequestDto, PagedResultDto<VideoResponseDto>>
{
    private readonly IVideoRepositoryEF _repository = repository;

    /// <inheritdoc/>
    public async Task<RequestResult<PagedResultDto<VideoResponseDto>>> ExecuteAsync(VideoSearchRequestDto request)
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
            return RequestResult<PagedResultDto<VideoResponseDto>>.WithError(result.Message ?? "Failed to search videos");
        }

        var responseDtos = result.Data.Items.Select(VideoResponseDto.FromVideo).ToList();

        var pagedResult = PagedResultDto<VideoResponseDto>.Create(responseDtos, request.Page, request.ItemsPerPage, result.Data.TotalCount);

        return RequestResult<PagedResultDto<VideoResponseDto>>.Success(pagedResult);
    }
}
