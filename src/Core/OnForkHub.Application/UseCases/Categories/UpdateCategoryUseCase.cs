namespace OnForkHub.Application.UseCases.Categories;

public class UpdateCategoryUseCase(ICategoryRepositoryEF categoryRepositoryEF, IEntityValidator<Category> validator)
    : IUseCase<CategoryUpdateRequestDto, Category>
{
    private readonly ICategoryRepositoryEF _categoryRepositoryEF = categoryRepositoryEF;
    private readonly IEntityValidator<Category> _validator = validator;

    public async Task<RequestResult<Category>> ExecuteAsync(CategoryUpdateRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = Name.Create(request.Category.Name);
        var category = await _categoryRepositoryEF.GetByIdAsync(request.Id);
        if (category.Status != EResultStatus.Success || category.Data is null)
        {
            return RequestResult<Category>.WithError("Category not found");
        }

        var updateResult = category.Data.UpdateCategory(name, request.Category.Description);
        if (updateResult.Status != EResultStatus.Success)
        {
            return RequestResult<Category>.WithError(updateResult.ToString());
        }

        var validationResult = _validator.ValidateUpdate(category.Data);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new RequestValidation(e.Field, e.Message)).ToArray();
            return RequestResult<Category>.WithValidations(errors);
        }

        var result = await _categoryRepositoryEF.UpdateAsync(category.Data);
        return result.Status != EResultStatus.Success || result.Data is null
            ? RequestResult<Category>.WithError("Failed to update category")
            : RequestResult<Category>.Success(result.Data);
    }
}
