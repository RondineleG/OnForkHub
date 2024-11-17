namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationResult
{
    bool IsValid { get; }
    bool HasError { get; }
    IReadOnlyCollection<ValidationErrorMessage> Errors { get; }
    IDictionary<string, object> Metadata { get; }
    string ErrorMessage { get; }

    IValidationResult AddError(string message, string field = "");
    IValidationResult Merge(IValidationResult other);
    ValidationResult AddError(string message, string field = "", string? source = null);
    ValidationResult AddErrorIf(bool condition, string message, string field = "");
    ValidationResult AddErrorIfNull<T>(T? value, string message, string field = "")
        where T : class;
    ValidationResult AddErrorIfNullOrWhiteSpace(string value, string message, string field = "");
    ValidationResult AddErrorIfNullOrEmpty(string value, string message, string field = "");
    ValidationResult AddErrors(IEnumerable<(string Message, string Field)> errors);
    ValidationResult Merge(ValidationResult other);
    void ThrowIfInvalid(string? customMessage = null);
    Task ThrowIfInvalidAsync(string? customMessage = null);
    T? GetMetadata<T>(string key)
        where T : class;
    ValidationResult ThrowIfInvalidAndReturn();
}
