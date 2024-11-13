namespace OnForkHub.Core.Interfaces.Validations;

public interface IEntityValidator<in T>
    where T : BaseEntity
{
    CustomValidationResult Validate(T entity);
}
