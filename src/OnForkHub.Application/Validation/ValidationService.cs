namespace OnForkHub.Application.Validation;

public sealed class ValidationService : IValidationService
{
    public void AddErrorIfInvalid(CustomValidationResult validationResult, string contexto, RequestResult requestResult)
    {
        throw new NotImplementedException();
    }

    public void Validate<T>(T entity, Func<T, CustomValidationResult> funcValidationResult, string entityName, RequestResult requestResult)
    {
        throw new NotImplementedException();
    }

    public void Validate<T>(
        IEnumerable<T> entities,
        Func<T, CustomValidationResult> funcValidationResult,
        string entityName,
        RequestResult requestResult
    )
    {
        throw new NotImplementedException();
    }

    public CustomValidationResult Validate(string value, string regexPattern, string fieldName)
    {
        throw new NotImplementedException();
    }

    public CustomValidationResult Validate(DateTime dateTimeStart, DateTime dateTimeFinish, string fieldName)
    {
        throw new NotImplementedException();
    }
}
