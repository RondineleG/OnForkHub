namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationResult
{
    string ErrorMessage { get; }

    IReadOnlyCollection<ValidationErrorMessage> Errors { get; }

    bool HasError { get; }

    bool IsValid { get; }

    IDictionary<string, object> Metadata { get; }

    IValidationResult AddError(string message, string field = "", string? source = null);

    IValidationResult AddErrorIf(Func<bool> condition, string message, string field = "");

    T? GetMetadata<T>(string key)
        where T : class;

    IValidationResult Merge(IValidationResult other);

    void ThrowIfInvalid(string? customMessage = null);

    IValidationResult ThrowIfInvalidAndReturn();

    Task ThrowIfInvalidAsync(string? customMessage = null);

    Task<ValidationResult> ValidateAsync(Func<Task<bool>> predicate, string message, string field = "");
}