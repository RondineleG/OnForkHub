namespace OnForkHub.Core.Exceptions;

public class DomainException(string message) : Exception(message)
{
    public static void ThrowErrorWhen(Func<bool> hasError, string message)
    {
        if (hasError())
        {
            throw new DomainException(message);
        }
    }

    public static void ThrowWhenInvalid(params ValidationResult[] validations)
    {
        var combinedResult = ValidationResult.Combine(validations);
        if (combinedResult.Errors.Count > 0)
        {
            throw new DomainException(combinedResult.ErrorMessage);
        }
    }

    public static ValidationResult Validate(Func<bool> hasError, string message)
    {
        return ValidationResult.Validate(hasError, message);
    }
}
