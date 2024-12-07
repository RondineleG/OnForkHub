namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationService<T>
    where T : BaseEntity
{
    IEntityValidator<T> Validator { get; }
    IValidationBuilder<T> Builder { get; }
    IValidationResult Validate(T entity);
    IValidationResult ValidateUpdate(T entity);
    IValidationService<T> AddRule(IValidationRule<T> rule);
    IValidationService<T> AddValidation(Func<T, ValidationResult> validation);
    IValidationService<T> WithErrorHandler(Action<ValidationErrorMessage> errorHandler);
}