using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationService
{
    void AddErrorIfInvalid(ValidationResult validationResult, string contexto, RequestResult requestResult);
    void Validate<T>(T entity, Func<T, ValidationResult> funcValidationResult, string entityName, RequestResult requestResult);
    void Validate<T>(IEnumerable<T> entities, Func<T, ValidationResult> funcValidationResult, string entityName, RequestResult requestResult);
    public ValidationResult Validate(string value, string regexPattern, string fieldName);
    public ValidationResult Validate(DateTime dateTimeStart, DateTime dateTimeFinish, string fieldName);
}
