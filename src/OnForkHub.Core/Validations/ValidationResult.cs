using System.Collections.ObjectModel;

namespace OnForkHub.Core.Validations;

public sealed class ValidationResult
{
    private readonly List<ValidationErrorMessage> _errors;

    public bool IsValid => !_errors.Any();
    public bool HasError => !IsValid;
    public string ErrorMessage => string.Join("; ", _errors.Select(e => e.Message));
    public IReadOnlyCollection<ValidationErrorMessage> Errors => new ReadOnlyCollection<ValidationErrorMessage>(_errors);

    public ValidationResult()
    {
        _errors = [];
    }

    private ValidationResult(string errorMessage, string fieldName = "") : this()
    {
        AddError(errorMessage, fieldName);
    }

    public ValidationResult AddError(string errorMessage, string fieldName = "")
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        _errors.Add(new ValidationErrorMessage(errorMessage, fieldName));
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

    public ValidationResult AddErrorIfNull<T>(T value, string errorMessage, string fieldName = "") where T : class
        => AddErrorIf(value == null, errorMessage, fieldName);

    public ValidationResult AddErrorIfNullOrEmpty(string value, string errorMessage, string fieldName = "")
        => AddErrorIf(string.IsNullOrEmpty(value), errorMessage, fieldName);

    public ValidationResult AddErrorIfNullOrWhiteSpace(string value, string errorMessage, string fieldName = "")
        => AddErrorIf(string.IsNullOrWhiteSpace(value), errorMessage, fieldName);

    public ValidationResult Merge(ValidationResult other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        _errors.AddRange(other._errors);
        return this;
    }

    public void ThrowIfInvalid()
    {
        if (HasError)
            throw new DomainException(ErrorMessage);
    }

    public ValidationResult ThrowIfInvalidAndReturn()
    {
        ThrowIfInvalid();
        return this;
    }


    public static ValidationResult Success() => new();

    public static ValidationResult Failure(string errorMessage, string fieldName = "")
        => new(errorMessage, fieldName);

    public static ValidationResult Combine(params ValidationResult[] validations)
    {
        if (validations == null || !validations.Any())
            return Success();

        var result = new ValidationResult();
        foreach (var validation in validations.Where(v => v != null))
        {
            result.Merge(validation);
        }
        return result;
    }

    public static ValidationResult Validate(Func<bool> predicate, string errorMessage, string fieldName = "")
      => predicate() ? Failure(errorMessage, fieldName) : Success();



    public static implicit operator bool(ValidationResult validation)
        => validation?.IsValid ?? false;

    public static ValidationResult operator &(ValidationResult left, ValidationResult right)
    {
        if (left == null) return right ?? Success();
        if (right == null) return left;

        return !left.IsValid ? left : left.Merge(right);
    }

    public static ValidationResult operator |(ValidationResult left, ValidationResult right)
    {
        if (left == null) return right ?? Success();
        if (right == null) return left;

        return left.IsValid ? left : right;
    }

}
