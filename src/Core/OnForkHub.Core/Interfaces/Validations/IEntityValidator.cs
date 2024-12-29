namespace OnForkHub.Core.Interfaces.Validations;

public interface IEntityValidator<in T>
    where T : BaseEntity
{
    IValidationResult Validate(T entity);
    IValidationResult ValidateUpdate(T entity);
}

public class EntityValidator<T> : IEntityValidator<T>
    where T : BaseEntity
{
    public IValidationResult Validate(T entity)
    {
        var result = new ValidationResult();

        if (entity == null)
        {
            result.AddError($"{typeof(T).Name} cannot be null.", nameof(entity));
        }

        return result;
    }

    public IValidationResult ValidateUpdate(T entity)
    {
        var result = Validate(entity);

        if (entity?.Id is null)
        {
            result.AddError($"{typeof(T).Name} ID is required for updates.", nameof(entity.Id));
        }

        return result;
    }
}