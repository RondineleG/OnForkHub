namespace OnForkHub.Application.UseCases.Categories.V3;

public class UpdateCategoryUseCase(ICategoryServiceRavenDB categoryServiceRavenDB, IEntityValidator<Category> validator)
    : IUseCase<CategoryRequestDto, Category>
{
    private readonly ICategoryServiceRavenDB _categoryServiceRavenDB = categoryServiceRavenDB;

    private readonly IEntityValidator<Category> _validator = validator;

    public async Task<RequestResult<Category>> ExecuteAsync(CategoryRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = Name.Create(request.Name);
        var categoryResult = Category.Create(name, request.Description);
        if ((categoryResult.Status != EResultStatus.Success) || (categoryResult.Data is null))
        {
            return RequestResult<Category>.WithError(categoryResult.ToString());
        }

        var validationResult = _validator.Validate(categoryResult.Data);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new RequestValidation(e.Field, e.Message)).ToArray();
            return RequestResult<Category>.WithValidations(errors);
        }

        var result = await _categoryServiceRavenDB.UpdateAsync(categoryResult.Data);
        return ((result.Status != EResultStatus.Success) || (result.Data is null))
            ? RequestResult<Category>.WithError("Failed to create category")
            : RequestResult<Category>.Success(result.Data);
    }
}
