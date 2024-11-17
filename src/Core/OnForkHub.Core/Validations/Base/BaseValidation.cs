using OnForkHub.Core.Interfaces.Validations;

namespace OnForkHub.Core.Validations.Base;

public abstract class BaseValidation<T>(IValidationBuilder builder) : IEntityValidator<T>
    where T : BaseEntity
{
    private readonly IValidationBuilder _builder = builder ?? throw new ArgumentNullException(nameof(builder));

    public virtual IValidationResult Validate(T entity)
    {
        return entity == null ? ValidationResult.Failure(message: $"{typeof(T).Name} cannot be null", field: nameof(T)) : ValidateEntity(entity);
    }

    public virtual IValidationResult ValidateUpdate(T entity)
    {
        var result = Validate(entity);
        if (entity?.Id <= 0)
        {
            result.AddError($"{typeof(T).Name} ID is required for updates", nameof(entity.Id));
        }
        return result;
    }

    protected abstract IValidationResult ValidateEntity(T entity);

    protected IValidationResult ValidateStringLength(string? value, string fieldName, int maxLength)
    {
        return _builder.WithField(fieldName, value).NotNull().NotEmpty().MaxLength(maxLength).Validate();
    }

    protected IValidationResult ValidateRequired(object? value, string fieldName)
    {
        return _builder.WithField(fieldName, value).NotNull().Validate();
    }
}
