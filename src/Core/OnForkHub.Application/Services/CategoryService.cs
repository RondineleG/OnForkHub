namespace OnForkHub.Application.Services;

public class CategoryService(ICategoryRepositoryEF categoryRepository, IValidationService<Category> validationService) : BaseService, ICategoryService
{
    private readonly ICategoryRepositoryEF _categoryRepository = categoryRepository;

    private readonly IValidationService<Category> _validationService = validationService;

    public Task<RequestResult<Category>> CreateAsync(Category category)
    {
        return ExecuteWithValidationAsync(category, _categoryRepository.CreateAsync, _validationService);
    }

    public async Task<RequestResult<Category>> DeleteAsync(long id)
    {
        return await ExecuteAsync(async () =>
        {
            var categoryResult = await _categoryRepository.GetByIdAsync(id);

            return !categoryResult.Status.Equals(EResultStatus.Success) ? categoryResult : await _categoryRepository.DeleteAsync(id);
        });
    }

    public Task<RequestResult<IEnumerable<Category>>> GetAllAsync(int page, int size)
    {
        return ExecuteAsync(async () => await _categoryRepository.GetAllAsync(page, size));
    }

    public Task<RequestResult<Category>> GetByIdAsync(long id)
    {
        return ExecuteAsync(async () =>
        {
            var result = await _categoryRepository.GetByIdAsync(id);
            return !result.Status.Equals(EResultStatus.Success) ? RequestResult<Category>.WithError($"Category with id {id} not found") : result;
        });
    }

    public Task<RequestResult<Category>> UpdateAsync(Category category)
    {
        return ExecuteWithValidationAsync(category, _categoryRepository.UpdateAsync, _validationService, true);
    }
}
