using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public static void When(bool hasError, string message)
    {
        if (hasError)
            throw new DomainException(message);
    }

    public static ValidationResult Validate(Func<bool> hasError, string message) => ValidationResult.Validate(hasError, message);

    public static void ThrowWhenInvalid(params ValidationResult[] validations)
    {
        var combinedResult = ValidationResult.Combine(validations);
        if (combinedResult.HasError)
            throw new DomainException(combinedResult.ErrorMessage);
    }
}
