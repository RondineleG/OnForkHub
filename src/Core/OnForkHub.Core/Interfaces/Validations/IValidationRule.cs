namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationRule<in T>
    where T : BaseEntity
{
    string PropertyName { get; }
    IValidationResult Validate(T entity);
}
