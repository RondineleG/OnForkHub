namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationRule<in T>
    where T : BaseEntity
{
    string PropertyName { get; }
    ValidationResult Validate(T entity);
}
