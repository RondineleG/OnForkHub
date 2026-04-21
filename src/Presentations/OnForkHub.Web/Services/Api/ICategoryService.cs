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
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<PagedResult<CategoryDto>> GetCategoriesAsync(int page = 1, int pageSize = 20, string? search = null);

    /// <summary>
    /// Gets all categories (no pagination, for dropdowns).
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<CategoryDto>> GetAllCategoriesAsync();

    /// <summary>
    /// Gets a single category by its ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<CategoryDto?> GetCategoryByIdAsync(long id);

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<CategoryDto> CreateCategoryAsync(string name, string description);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<CategoryDto> UpdateCategoryAsync(long id, string name, string description);

    /// <summary>
    /// Deletes a category by its ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteCategoryAsync(long id);
}
