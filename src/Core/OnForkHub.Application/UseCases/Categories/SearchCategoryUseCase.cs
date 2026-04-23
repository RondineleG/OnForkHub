namespace OnForkHub.Application.UseCases.Categories;

using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.Dtos.Category.Request;
using OnForkHub.Application.Dtos.Category.Response;
using OnForkHub.Core.Interfaces.Repositories;

/// <summary>
/// Use case for searching categories with filters.
/// </summary>
public class SearchCategoryUseCase(ICategoryRepositoryEF repository) : IUseCase<CategorySearchRequestDto, PagedResultDto<CategoryResponseDto>>
{
    private readonly ICategoryRepositoryEF _repository = repository;

    /// <inheritdoc/>
    public async Task<RequestResult<PagedResultDto<CategoryResponseDto>>> ExecuteAsync(CategorySearchRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _repository.SearchAsync(
            request.SearchTerm,
            (int)request.SortBy,
            request.SortDescending,
            request.Page,
            request.ItemsPerPage
        );

        if (result.Status != EResultStatus.Success || result.Data.Items is null)
        {
            return RequestResult<PagedResultDto<CategoryResponseDto>>.WithError(result.Message ?? "Failed to search categories");
        }

        var responseDtos = result.Data.Items.Select(CategoryResponseDto.FromCategory).ToList();

        var pagedResult = PagedResultDto<CategoryResponseDto>.Create(responseDtos, request.Page, request.ItemsPerPage, result.Data.TotalCount);

        return RequestResult<PagedResultDto<CategoryResponseDto>>.Success(pagedResult);
    }
}
