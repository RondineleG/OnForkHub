namespace OnForkHub.Web.Services.Api;

using OnForkHub.Web.Models;

/// <summary>
/// Service contract for category API operations.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets a paginated list of categories.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<PagedResult<CategoryResponse>> GetCategoriesAsync(int page = 1, int pageSize = 20, string? search = null);

    /// <summary>
    /// Gets all categories (no pagination, for dropdowns).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<List<CategoryResponse>> GetAllCategoriesAsync();

    /// <summary>
    /// Gets a single category by its ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<CategoryResponse?> GetCategoryByIdAsync(long id);

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<CategoryResponse> CreateCategoryAsync(string name, string description);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<CategoryResponse> UpdateCategoryAsync(long id, string name, string description);

    /// <summary>
    /// Deletes a category by its ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteCategoryAsync(long id);
}
