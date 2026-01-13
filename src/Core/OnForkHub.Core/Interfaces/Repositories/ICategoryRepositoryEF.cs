namespace OnForkHub.Core.Interfaces.Repositories;

public interface ICategoryRepositoryEF
{
    Task<RequestResult<Category>> CreateAsync(Category category);

    Task<RequestResult<Category>> DeleteAsync(long id);

    Task<RequestResult<IEnumerable<Category>>> GetAllAsync(int page, int size);

    Task<RequestResult<Category>> GetByIdAsync(long id);

    Task<RequestResult<Category>> UpdateAsync(Category category);

    /// <summary>
    /// Searches categories with filters.
    /// </summary>
    /// <param name="searchTerm">The search term for category name.</param>
    /// <param name="sortBy">Sort field (0=Name, 1=CreatedAt).</param>
    /// <param name="sortDescending">Sort direction.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A paginated list of categories with total count.</returns>
    Task<RequestResult<(IEnumerable<Category> Items, int TotalCount)>> SearchAsync(
        string? searchTerm,
        int sortBy,
        bool sortDescending,
        int page,
        int pageSize
    );

    /// <summary>
    /// Gets the total count of categories.
    /// </summary>
    /// <returns>The total count.</returns>
    Task<int> GetTotalCountAsync();
}
