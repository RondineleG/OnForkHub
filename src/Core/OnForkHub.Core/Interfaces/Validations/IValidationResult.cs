namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationResult
{
    bool IsValid { get; }
    bool HasError { get; }
    IReadOnlyCollection<ValidationErrorMessage> Errors { get; }
    IDictionary<string, object> Metadata { get; }
    string ErrorMessage { get; }

    IValidationResult AddError(string message, string field = "", string? source = null);
    IValidationResult AddErrorIf(Func<bool> condition, string message, string field = "");
    IValidationResult Merge(IValidationResult other);
    void ThrowIfInvalid(string? customMessage = null);
    Task ThrowIfInvalidAsync(string? customMessage = null);

    T? GetMetadata<T>(string key)
        where T : class;

    IValidationResult ThrowIfInvalidAndReturn();
    Task<ValidationResult> ValidateAsync(Func<Task<bool>> predicate, string message, string field = "");
}
