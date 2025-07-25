namespace OnForkHub.Core.Exceptions;

public class DomainException(string message, string errorCode = "DOMAIN_ERROR") : CustomException(message, errorCode)
{
    public static void ThrowErrorWhen(Func<bool> hasError, string message, string errorCode = "DOMAIN_VALIDATION_ERROR")
    {
        if (hasError())
        {
            throw new DomainException(message, errorCode);
        }
    }

    public static void ThrowWhenInvalid(params ValidationResult[] validations)
    {
        var combinedResult = ValidationResult.Combine(validations);
        if (combinedResult.Errors.Count > 0)
        {
            throw new DomainException(combinedResult.ErrorMessage, "DOMAIN_VALIDATION_ERROR");
        }
    }

    public static IValidationResult Validate(Func<bool> condition, string message)
    {
        return ValidationResult.Success().AddErrorIf(condition, message);
    }
}
