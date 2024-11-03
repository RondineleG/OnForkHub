using System.Collections.ObjectModel;

namespace OnForkHub.Core.Validations;

public sealed class ValidationResult
{
    private readonly List<ValidationErrorMessage> _errors;

    public bool IsValid => _errors.Count == 0;

    public bool HasError => !IsValid;
    public string ErrorMessage => string.Join("; ", _errors.Select(e => e.Message));

    public IReadOnlyCollection<ValidationErrorMessage> Errors =>
        new ReadOnlyCollection<ValidationErrorMessage>(_errors);

    public ValidationResult()
    {
        _errors = [];
    }

    private ValidationResult(string errorMessage, string fieldName = "")
        : this()
    {
        AddError(errorMessage, fieldName);
    }

    public ValidationResult AddError(string errorMessage, string fieldName = "")
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("A mensagem de erro não pode estar vazia", nameof(errorMessage));
        }

        _errors.Add(new ValidationErrorMessage(errorMessage, fieldName));
        return this;
    }

    public void ThrowIfInvalid()
    {
        if (HasError)
        {
            throw new DomainException(ErrorMessage);
        }
    }

    public void ThrowIfInvalid(string errorMessage)
    {
        if (HasError)
        {
            throw new DomainException(errorMessage);
        }
    }

    public ValidationResult ThrowIfInvalidAndReturn()
    {
        ThrowIfInvalid();
        return this;
    }

    public ValidationResult AddErrors(IEnumerable<(string Message, string Field)> errors)
    {
        foreach (var (message, field) in errors)
        {
            AddError(message, field);
        }
        return this;
    }

    public ValidationResult AddErrorIf(bool condition, string errorMessage, string fieldName = "")
    {
        if (condition)
        {
            AddError(errorMessage, fieldName);
        }
        return this;
    }

    public ValidationResult AddErrorIfNull<T>(T value, string errorMessage, string fieldName = "")
        where T : class
    {
        return AddErrorIf(value == null, errorMessage, fieldName);
    }

    public ValidationResult AddErrorIfNullOrEmpty(string value, string errorMessage, string fieldName = "")
    {
        return AddErrorIf(string.IsNullOrEmpty(value), errorMessage, fieldName);
    }

    public ValidationResult AddErrorIfNullOrWhiteSpace(string value, string errorMessage, string fieldName = "")
    {
        return AddErrorIf(string.IsNullOrWhiteSpace(value), errorMessage, fieldName);
    }

    public ValidationResult Merge(ValidationResult other)
    {
        ArgumentNullException.ThrowIfNull(other);

        _errors.AddRange(other._errors);
        return this;
    }

    public static void ThrowErrorIf(Func<bool> hasError, string message)
    {
        if (hasError())
        {
            throw new DomainException(message);
        }
    }

    public static ValidationResult Success()
    {
        return new();
    }

    public static ValidationResult Failure(string errorMessage, string fieldName = "")
    {
        return new(errorMessage, fieldName);
    }

    public static ValidationResult Combine(params ValidationResult[] validations)
    {
        if (validations == null || validations.Length == 0)
        {
            return Success();
        }

        var result = new ValidationResult();
        foreach (var validation in validations.Where(v => v != null))
        {
            result.Merge(validation);
        }
        return result;
    }

    public static ValidationResult Validate(Func<bool> predicate, string errorMessage, string fieldName = "")
    {
        return predicate() ? Failure(errorMessage, fieldName) : Success();
    }

    public static implicit operator bool(ValidationResult validation)
    {
        return validation?.IsValid ?? false;
    }

    public static ValidationResult operator &(ValidationResult left, ValidationResult right)
    {
        return left == null ? right ?? Success()
            : right == null ? left
            : !left.IsValid ? left
            : left.Merge(right);
    }

    public static ValidationResult operator |(ValidationResult left, ValidationResult right)
    {
        return left == null ? right ?? Success()
            : right == null ? left
            : left.IsValid ? left
            : right;
    }
}
