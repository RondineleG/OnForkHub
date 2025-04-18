namespace OnForkHub.Core.Validations;

public class ValidationBuilder<T> : IValidationBuilder<T>
    where T : BaseEntity
{
    private readonly ValidationResult _result = new();

    private string _currentField = string.Empty;

    private object? _currentValue;

    public IValidationBuilder<T> Custom(Func<object?, bool> validation, string message)
    {
        if (!validation(_currentValue))
        {
            _result.AddError(message, _currentField);
        }

        return this;
    }

    public async Task<IValidationBuilder<T>> CustomAsync(Func<object?, Task<bool>> validation, string message)
    {
        if (!await validation(_currentValue))
        {
            _result.AddError(message, _currentField);
        }

        return this;
    }

    public IValidationBuilder<T> Ensure(Func<bool> predicate, string message)
    {
        if (!predicate())
        {
            _result.AddError(message, _currentField);
        }

        return this;
    }

    public async Task<IValidationBuilder<T>> EnsureAsync(Func<Task<bool>> predicate, string message)
    {
        if (!await predicate())
        {
            _result.AddError(message, _currentField);
        }

        return this;
    }

    public IValidationBuilder<T> Length(int exactLength, string? message = null)
    {
        if (_currentValue is string str && str.Length != exactLength)
        {
            _result.AddError(message ?? $"{_currentField} must have exactly {exactLength} characters", _currentField);
        }

        return this;
    }

    public IValidationBuilder<T> Matches(string pattern, string? message = null)
    {
        if (_currentValue is string str && !Regex.IsMatch(str, pattern))
        {
            _result.AddError(message ?? $"{_currentField} is not in the correct format", _currentField);
        }

        return this;
    }

    public IValidationBuilder<T> MaxLength(int length, string? message = null)
    {
        if (_currentValue is string str && str.Length > length)
        {
            _result.AddError(message ?? $"{_currentField} must have at most {length} characters", _currentField);
        }

        return this;
    }

    public IValidationBuilder<T> MinLength(int length, string? message = null)
    {
        if (_currentValue is string str && str.Length < length)
        {
            _result.AddError(message ?? $"{_currentField} deve ter no mínimo {length} caracteres", _currentField);
        }

        return this;
    }

    public IValidationBuilder<T> NotEmpty(string? message = null)
    {
        if (_currentValue is null || (_currentValue is string str && string.IsNullOrEmpty(str)))
        {
            _result.AddError(message ?? $"{_currentField} cannot be empty", _currentField);
        }

        return this;
    }

    public IValidationBuilder<T> NotNull(string? message = null)
    {
        if (_currentValue is null)
        {
            _result.AddError(message ?? $"{_currentField} is required", _currentField);
        }

        return this;
    }

    public IValidationBuilder<T> NotWhiteSpace(string? message = null)
    {
        if (_currentValue is string str && string.IsNullOrWhiteSpace(str))
        {
            _result.AddError(message ?? $"{_currentField} cannot be blank", _currentField);
        }

        return this;
    }

    public IValidationBuilder<T> Range<TRange>(TRange min, TRange max, string? message = null)
        where TRange : IComparable<TRange>
    {
        if (_currentValue is TRange value && (value.CompareTo(min) < 0 || value.CompareTo(max) > 0))
        {
            _result.AddError(message ?? $"{_currentField} must be between {min} and {max}", _currentField);
        }

        return this;
    }

    public IValidationResult Validate()
    {
        return _result;
    }

    public IValidationBuilder<T> WithField(string fieldName, object? value = null)
    {
        _currentField = fieldName;
        _currentValue = value;
        return this;
    }

    public IValidationBuilder<T> WithMetadata<TMetadata>(string key, TMetadata value)
    {
        _result.Metadata[key] = value!;
        return this;
    }
}