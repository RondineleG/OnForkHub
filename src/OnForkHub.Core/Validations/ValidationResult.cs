namespace OnForkHub.Core.Validations;

public class ValidationResult
{
    public bool IsValid { get; }
    public string ErrorMessage { get; }
    public bool HasError => !IsValid;

    private ValidationResult(bool isValid, string errorMessage = "")
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);

    public static implicit operator bool(ValidationResult validation) => validation.IsValid;

    public static ValidationResult Combine(params ValidationResult[] validations)
    {
        var errors = validations
            .Where(v => v.HasError)
            .Select(v => v.ErrorMessage)
            .ToArray();

        if (errors.Any())
        {
            return Failure(string.Join("; ", errors));
        }

        return Success();
    }

    public static ValidationResult Validate(bool hasError, string message)
        => hasError ? Failure(message) : Success();

    public void ThrowIfInvalid()
    {
        if (HasError)
            throw new DomainException(ErrorMessage);
    }

    public static ValidationResult operator &(ValidationResult a, ValidationResult b) => !a.IsValid ? a : b;
}




