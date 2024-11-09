namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationService
{
    void AddErrorIfInvalid(CustomValidationResult validationResult, string contexto, RequestResult requestResult);
    void Validate<T>(T entity, Func<T, CustomValidationResult> funcValidationResult, string entityName, RequestResult requestResult);
    void Validate<T>(IEnumerable<T> entities, Func<T, CustomValidationResult> funcValidationResult, string entityName, RequestResult requestResult);
    public CustomValidationResult Validate(string value, string regexPattern, string fieldName);
    public CustomValidationResult Validate(DateTime dateTimeStart, DateTime dateTimeFinish, string fieldName);
}
