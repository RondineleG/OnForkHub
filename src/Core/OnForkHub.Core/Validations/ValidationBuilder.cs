using OnForkHub.Core.Interfaces.Validations;

namespace OnForkHub.Core.Validations;

public class ValidationBuilder : IValidationBuilder
{
    private readonly ValidationResult _result = new();
    private string _currentField = string.Empty;
    private object? _currentValue;

    public IValidationBuilder WithField(string fieldName, object? value = null)
    {
        _currentField = fieldName;
        _currentValue = value;
        return this;
    }

    public IValidationBuilder NotNull(string? message = null)
    {
        if (_currentValue is null)
        {
            _result.AddError(message ?? $"{_currentField} is required", _currentField);
        }
        return this;
    }

    public IValidationBuilder NotEmpty(string? message = null)
    {
        if (_currentValue is string str && string.IsNullOrEmpty(str))
        {
            _result.AddError(message ?? $"{_currentField} não pode estar vazio", _currentField);
        }
        return this;
    }

    public IValidationBuilder NotWhiteSpace(string? message = null)
    {
        if (_currentValue is string str && string.IsNullOrWhiteSpace(str))
        {
            _result.AddError(message ?? $"{_currentField} não pode estar em branco", _currentField);
        }
        return this;
    }

    public IValidationBuilder MinLength(int length, string? message = null)
    {
        if (_currentValue is string str && str.Length < length)
        {
            _result.AddError(message ?? $"{_currentField} deve ter no mínimo {length} caracteres", _currentField);
        }
        return this;
    }

    public IValidationBuilder MaxLength(int length, string? message = null)
    {
        if (_currentValue is string str && str.Length > length)
        {
            _result.AddError(message ?? $"{_currentField} deve ter no máximo {length} caracteres", _currentField);
        }
        return this;
    }

    public IValidationBuilder Length(int exactLength, string? message = null)
    {
        if (_currentValue is string str && str.Length != exactLength)
        {
            _result.AddError(message ?? $"{_currentField} deve ter exatamente {exactLength} caracteres", _currentField);
        }
        return this;
    }

    public IValidationBuilder Range<T>(T min, T max, string? message = null)
        where T : IComparable<T>
    {
        if (_currentValue is T value && (value.CompareTo(min) < 0 || value.CompareTo(max) > 0))
        {
            _result.AddError(message ?? $"{_currentField} deve estar entre {min} e {max}", _currentField);
        }
        return this;
    }

    public IValidationBuilder Matches(string pattern, string? message = null)
    {
        if (_currentValue is string str && !Regex.IsMatch(str, pattern))
        {
            _result.AddError(message ?? $"{_currentField} não está no formato correto", _currentField);
        }
        return this;
    }

    public IValidationBuilder Custom(Func<object?, bool> validation, string message)
    {
        if (!validation(_currentValue))
        {
            _result.AddError(message, _currentField);
        }
        return this;
    }

    public async Task<IValidationBuilder> CustomAsync(Func<object?, Task<bool>> validation, string message)
    {
        if (!await validation(_currentValue))
        {
            _result.AddError(message, _currentField);
        }
        return this;
    }

    public IValidationBuilder WithMetadata<T>(string key, T value)
    {
        _result.Metadata[key] = value!;
        return this;
    }

    public IValidationBuilder Ensure(Func<bool> predicate, string message)
    {
        if (!predicate())
        {
            _result.AddError(message, _currentField);
        }
        return this;
    }

    public async Task<IValidationBuilder> EnsureAsync(Func<Task<bool>> predicate, string message)
    {
        if (!await predicate())
        {
            _result.AddError(message, _currentField);
        }
        return this;
    }

    public IValidationResult Validate()
    {
        return _result;
    }
}
