using System.Runtime.CompilerServices;
using OnForkHub.Core.Interfaces.Validations;

namespace OnForkHub.Core.Validations;

public sealed class CustomValidationResult : IValidationResult
{
    private readonly List<ValidationErrorMessage> _errors;
    private readonly Dictionary<string, object> _metadata;

    public CustomValidationResult()
    {
        _errors = [];
        _metadata = [];
    }

    public bool IsValid => _errors.Count == 0;
    public bool HasError => !IsValid;
    public IReadOnlyCollection<ValidationErrorMessage> Errors => new ReadOnlyCollection<ValidationErrorMessage>(_errors);
    public IDictionary<string, object> Metadata => _metadata;

    public string ErrorMessage => string.Join("; ", _errors.Select(e => string.IsNullOrEmpty(e.Field) ? e.Message : $"{e.Field}: {e.Message}"));

    public IValidationResult AddError(string message, string field = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _errors.Add(new ValidationErrorMessage(message, field));
        return this;
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

    public static ValidationBuilder WithField()
    {
        return new();
    }

    public static IValidationBuilder WithField(string fieldName, object? value = null)
    {
        return new ValidationBuilder().WithField(fieldName, value);
    }

    public static CustomValidationResult Success()
    {
        return new();
    }

    public static CustomValidationResult Failure(string message, string field)
    {
        var result = new CustomValidationResult();
        result.AddError(message, field);
        return result;
    }

    public static CustomValidationResult Failure(string message, string field = "", [CallerMemberName] string? source = null)
    {
        var result = new CustomValidationResult();
        result.AddError(message, field, source);
        return result;
    }

    public CustomValidationResult AddError(string message, string field = "", string? source = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _errors.Add(new ValidationErrorMessage(message, field, source));
        return this;
    }

    public CustomValidationResult AddErrorIf(bool condition, string message, string field = "")
    {
        if (condition)
        {
            AddError(message, field);
        }
        return this;
    }

    public CustomValidationResult AddErrorI<T>(T value, string message, string field = "")
        where T : class
    {
        return AddErrorIf(value == null, message, field);
    }

    public CustomValidationResult AddErrorIfNull<T>(T? value, string message, string field = "")
        where T : class
    {
        return AddErrorIf(value is null, message, field);
    }

    public CustomValidationResult AddErrorIfNullOrEmpty(string value, string message, string field = "")
    {
        return AddErrorIf(string.IsNullOrEmpty(value), message, field);
    }

    public CustomValidationResult AddErrorIfNullOrWhiteSpace(string value, string message, string field = "")
    {
        return AddErrorIf(string.IsNullOrWhiteSpace(value), message, field);
    }

    public CustomValidationResult AddErrors(IEnumerable<(string Message, string Field)> errors)
    {
        foreach (var (message, field) in errors)
        {
            AddError(message, field);
        }
        return this;
    }

    public CustomValidationResult Merge(CustomValidationResult other)
    {
        ArgumentNullException.ThrowIfNull(other);
        _errors.AddRange(other._errors);

        foreach (var item in other._metadata)
        {
            _metadata[item.Key] = item.Value;
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

    public async Task ThrowIfInvalidAsync(string? customMessage = null)
    {
        await Task.Yield();
        ThrowIfInvalid(customMessage);
    }

    public T? GetMetadata<T>(string key)
        where T : class
    {
        return _metadata.TryGetValue(key, out var value) ? value as T : null;
    }

    public CustomValidationResult ThrowIfInvalidAndReturn()
    {
        ThrowIfInvalid();
        return this;
    }

    public static CustomValidationResult Combine(params CustomValidationResult[] results)
    {
        var combined = new CustomValidationResult();
        foreach (var result in results.Where(r => r != null))
        {
            combined.Merge(result);
        }
        return combined;
    }

    public static void ThrowErrorIf(Func<bool> hasError, string message)
    {
        if (hasError())
        {
            throw new DomainException(message);
        }
    }

    public static CustomValidationResult Validate(Func<bool> predicate, string message, string field = "")
    {
        return predicate() ? Failure(message, field) : Success();
    }

    public static CustomValidationResult operator &(CustomValidationResult left, CustomValidationResult right)
    {
        return GetAndOperatorResult(left, right);
    }

    public static CustomValidationResult operator |(CustomValidationResult left, CustomValidationResult right)
    {
        return left.IsValid ? left : right;
    }

    private static CustomValidationResult GetAndOperatorResult(CustomValidationResult left, CustomValidationResult right)
    {
        var result = left ?? right ?? Success();

        if ((left != null) && !left.IsValid)
        {
            result = left;
        }
        else if ((left != null) && (right != null))
        {
            result = left.Merge(right);
        }

        return result;
    }

    public static implicit operator bool(CustomValidationResult? validation)
    {
        return validation?.IsValid ?? false;
    }
}
