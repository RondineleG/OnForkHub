namespace OnForkHub.Core.Validations;

public sealed class CustomValidationResult
{
    private readonly List<ValidationErrorMessage> _errors;

    public CustomValidationResult()
    {
        _errors = new List<ValidationErrorMessage>();
    }

    private CustomValidationResult(string errorMessage, string fieldName = "")
        : this()
    {
        AddError(errorMessage, fieldName);
    }

    public string ErrorMessage => string.Join("; ", _errors.Select(e => e.Message));

    public IReadOnlyCollection<ValidationErrorMessage> Errors => new ReadOnlyCollection<ValidationErrorMessage>(_errors);

    public bool HasError => !IsValid;

    public bool IsValid => _errors.Count == 0;

    public static implicit operator bool(CustomValidationResult validation)
    {
        return validation?.IsValid ?? false;
    }

    public static CustomValidationResult operator &(CustomValidationResult left, CustomValidationResult right)
    {
        return GetAndOperatorResult(left, right);
    }

    public static CustomValidationResult operator |(CustomValidationResult left, CustomValidationResult right)
    {
        return left.IsValid ? left : right;
    }

    public static CustomValidationResult Combine(params CustomValidationResult[] validations)
    {
        if ((validations == null) || (validations.Length == 0))
        {
            return Success();
        }

        var result = new CustomValidationResult();
        foreach (var validation in validations.Where(v => v != null))
        {
            result.Merge(validation);
        }

        return result;
    }

    public static CustomValidationResult Failure(string errorMessage, string fieldName = "")
    {
        return new CustomValidationResult(errorMessage, fieldName);
    }

    public static CustomValidationResult Success()
    {
        return new CustomValidationResult();
    }

    public static void ThrowErrorIf(Func<bool> hasError, string message)
    {
        if (hasError())
        {
            throw new DomainException(message);
        }
    }

    public static CustomValidationResult Validate(Func<bool> predicate, string errorMessage, string fieldName = "")
    {
        return predicate() ? Failure(errorMessage, fieldName) : Success();
    }

    public CustomValidationResult AddError(string errorMessage, string fieldName = "")
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("The error message cannot be empty", nameof(errorMessage));
        }

        _errors.Add(new ValidationErrorMessage(errorMessage, fieldName));
        return this;
    }

    public CustomValidationResult AddErrorIf(bool condition, string errorMessage, string fieldName = "")
    {
        if (condition)
        {
            AddError(errorMessage, fieldName);
        }

        return this;
    }

    public CustomValidationResult AddErrorI<T>(T value, string errorMessage, string fieldName = "")
        where T : class
    {
        return AddErrorIf(value == null, errorMessage, fieldName);
    }

    public CustomValidationResult AddErrorIfNull<T>(T value, string errorMessage, string fieldName = "")
        where T : class
    {
        return AddErrorIf(value == null, errorMessage, fieldName);
    }

    public CustomValidationResult AddErrorIfNullOrEmpty(string value, string errorMessage, string fieldName = "")
    {
        return AddErrorIf(string.IsNullOrEmpty(value), errorMessage, fieldName);
    }

    public CustomValidationResult AddErrorIfNullOrWhiteSpace(string value, string errorMessage, string fieldName = "")
    {
        return AddErrorIf(string.IsNullOrWhiteSpace(value), errorMessage, fieldName);
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

    public CustomValidationResult ThrowIfInvalidAndReturn()
    {
        ThrowIfInvalid();
        return this;
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
}
