namespace OnForkHub.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository, IValidationService validationService)
    : ServiceBase(validationService),
        ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<RequestResult<Category>> CreateAsync(Category category)
    {
        return await ExecuteAsync(category, _categoryRepository.CreateAsync, ValidateCategory);
    }

    public async Task<RequestResult<Category>> UpdateAsync(Category category)
    {
        return await ExecuteAsync(category, _categoryRepository.UpdateAsync, ValidateCategory);
    }

    public async Task<RequestResult<Category>> DeleteAsync(long id)
    {
        var categoryResult = await GetByIdAsync(id);
        if (!categoryResult.Status.Equals(EResultStatus.Success))
        {
            return categoryResult;
        }

        return await ExecuteAsync(() => _categoryRepository.DeleteAsync(id));
    }

    public async Task<RequestResult<Category>> GetByIdAsync(long id)
    {
        return await ExecuteAsync(() => _categoryRepository.GetByIdAsync(id));
    }

    public async Task<RequestResult<IEnumerable<Category>>> GetAsync(int page, int size)
    {
        return await ExecuteAsync(() => _categoryRepository.GetAsync(page, size));
    }

    private static CustomValidationResult ValidateCategory(Category category)
    {
        var validationResult = new CustomValidationResult();

        validationResult
            .AddErrorIfNullOrWhiteSpace(category.Name.Value, "Category name is required", nameof(category.Name))
            .AddErrorIf(category.Description?.Length > 200, "Description cannot exceed 200 characters", nameof(category.Description));

        validationResult.Merge(category.Validate());

        return validationResult;
    }
}
