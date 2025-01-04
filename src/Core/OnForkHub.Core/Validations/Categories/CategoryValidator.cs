namespace OnForkHub.Core.Validations.Categories;

public class CategoryValidator : IEntityValidator<Category>
{
    public IValidationResult Validate(Category entity)
    {
        var result = new ValidationResult();
        if (entity == null)
        {
            result.AddError($"{nameof(Category)} cannot be null.", nameof(entity));
            return result;
        }

        if (entity.Name == null)
        {
            result.AddError($"{nameof(Category)} name is required.", nameof(entity.Name));
        }

        if (string.IsNullOrWhiteSpace(entity.Description))
        {
            result.AddError($"{nameof(Category)} description is required.", nameof(entity.Description));
        }
        else if (entity.Description.Length > 200)
        {
            result.AddError($"{nameof(Category)} description cannot exceed 200 characters.", nameof(entity.Description));
        }

        return result;
    }

    public IValidationResult ValidateUpdate(Category entity)
    {
        var result = Validate(entity);
        if (entity?.Id == null)
        {
            result.AddError($"{nameof(Category)} ID is required for updates.", nameof(entity.Id));
        }
        return result;
    }
}