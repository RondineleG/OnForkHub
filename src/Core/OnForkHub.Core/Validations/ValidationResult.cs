namespace OnForkHub.Core.Validations;

public sealed class ValidationResult : IValidationResult
{
    private readonly List<ValidationErrorMessage> _errors = [];
    private readonly Dictionary<string, object> _metadata = [];

    public string ErrorMessage => string.Join("; ", _errors.Select(e => string.IsNullOrEmpty(e.Field) ? e.Message : $"{e.Field}: {e.Message}"));
    public IReadOnlyCollection<ValidationErrorMessage> Errors => new ReadOnlyCollection<ValidationErrorMessage>(_errors);
    public bool HasError => !IsValid;
    public bool IsValid => _errors.Count == 0;
    public IDictionary<string, object> Metadata => _metadata;

    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var combined = new ValidationResult();
        foreach (var result in results.Where(r => r != null))
        {
            combined.Merge(result);
        }

        return combined;
    }

    public static ValidationResult Failure(string message, string field = "", string? source = null)
    {
        var result = new ValidationResult();
        result.AddError(message, field, source);
        return result;
    }

    public static implicit operator bool(ValidationResult? validation)
    {
        return validation?.IsValid ?? false;
    }

    public static ValidationResult operator &(ValidationResult left, ValidationResult right)
    {
        var result = left ?? right ?? Success();

        if (left?.HasError == true)
        {
            result = left;
        }
        else if (left != null && right != null)
        {
            result.Merge(right);
        }

        return result;
    }

    public static ValidationResult operator |(ValidationResult left, ValidationResult right)
    {
        return left?.IsValid == true ? left : right ?? Success();
    }

    public static ValidationResult Success()
    {
        return new ValidationResult();
    }

    public static ValidationResult Validate(Func<bool> predicate, string message, string field = "")
    {
        return predicate() ? Failure(message, field) : Success();
    }

    public IValidationResult AddError(string message, string field = "", string? source = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _errors.Add(new ValidationErrorMessage(message, field, source));
        return this;
    }

    public IValidationResult AddErrorIf(Func<bool> condition, string message, string field = "")
    {
        if (condition())
        {
            AddError(message, field);
        }

        return this;
    }

    public T? GetMetadata<T>(string key)
        where T : class
    {
        return _metadata.TryGetValue(key, out var value) ? value as T : null;
    }

    public IValidationResult Merge(IValidationResult other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (var error in other.Errors)
        {
            _errors.Add(error);
        }

        foreach (var meta in other.Metadata)
        {
            _metadata[meta.Key] = meta.Value;
        }

        return this;
    }

    public void ThrowIfInvalid(string? customMessage = null)
    {
        if (HasError)
        {
            throw new DomainException(customMessage ?? ErrorMessage);
        }
    }

    public IValidationResult ThrowIfInvalidAndReturn()
    {
        ThrowIfInvalid();
        return this;
    }

    public async Task ThrowIfInvalidAsync(string? customMessage = null)
    {
        await Task.Yield();
        ThrowIfInvalid(customMessage);
    }

    public async Task<ValidationResult> ValidateAsync(Func<Task<bool>> predicate, string message, string field = "")
    {
        return await predicate() ? Failure(message, field) : Success();
    }
}