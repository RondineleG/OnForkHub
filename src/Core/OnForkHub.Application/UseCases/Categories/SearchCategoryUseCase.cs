namespace OnForkHub.Application.UseCases.Categories;

using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.Dtos.Category.Request;
using OnForkHub.Application.Dtos.Category.Response;
using OnForkHub.Core.Interfaces.Repositories;

/// <summary>
/// Use case for searching categories with filters.
/// </summary>
public class SearchCategoryUseCase(ICategoryRepositoryEF repository) : IUseCase<CategorySearchRequest, PagedResponse<CategorySearchResponse>>
{
    private readonly ICategoryRepositoryEF _repository = repository;

    /// <inheritdoc/>
    public async Task<RequestResult<PagedResponse<CategorySearchResponse>>> ExecuteAsync(CategorySearchRequest request)
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
            return RequestResult<PagedResponse<CategorySearchResponse>>.WithError(result.Message ?? "Failed to search categories");
        }

        var responses = result.Data.Items.Select(CategorySearchResponse.FromCategory).ToList();

        var pagedResult = PagedResponse<CategorySearchResponse>.Create(responses, request.Page, request.ItemsPerPage, result.Data.TotalCount);

        return RequestResult<PagedResponse<CategorySearchResponse>>.Success(pagedResult);
    }
}
