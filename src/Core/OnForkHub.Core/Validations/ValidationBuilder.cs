using System.Diagnostics;

namespace OnForkHub.Core.Validations
{
    public class ValidationBuilder<T> : IValidationBuilder<T>
        where T : class
    {
        private readonly ValidationResult _result = new();

        private string _currentField = string.Empty;

        private object? _currentValue;

        public IValidationBuilder<T> Custom(Func<object?, bool> validation, string message)
        {
            ArgumentNullException.ThrowIfNull(validation);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            try
            {
                if (!validation(_currentValue))
                {
                    _result.AddError(message, _currentField);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Custom validation exception: {ex}");
                _result.AddError($"Custom validation failed: {ex.Message}", _currentField);
            }
            return this;
        }

        public async Task<IValidationBuilder<T>> CustomAsync(Func<object?, Task<bool>> validation, string message)
        {
            ArgumentNullException.ThrowIfNull(validation);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            try
            {
                if (!await validation(_currentValue).ConfigureAwait(false))
                {
                    _result.AddError(message, _currentField);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CustomAsync validation exception: {ex}");
                _result.AddError($"CustomAsync validation failed: {ex.Message}", _currentField);
            }
            return this;
        }

        public IValidationBuilder<T> Ensure(Func<bool> predicate, string message)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            try
            {
                if (!predicate())
                {
                    _result.AddError(message, _currentField);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ensure validation exception: {ex}");
                _result.AddError($"Ensure validation failed: {ex.Message}", _currentField);
            }
            return this;
        }

        public async Task<IValidationBuilder<T>> EnsureAsync(Func<Task<bool>> predicate, string message)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            try
            {
                if (!await predicate().ConfigureAwait(false))
                {
                    _result.AddError(message, _currentField);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EnsureAsync validation exception: {ex}");
                _result.AddError($"EnsureAsync validation failed: {ex.Message}", _currentField);
            }
            return this;
        }

        public IValidationBuilder<T> Length(int exactLength, string? message = null)
        {
            if (_currentValue is string str)
            {
                if (str.Length != exactLength)
                {
                    _result.AddError(message ?? $"{_currentField} must have exactly {exactLength} characters", _currentField);
                }
            }
            else
            {
                _result.AddError(message ?? $"{_currentField} must be a string for length validation", _currentField);
            }
            return this;
        }

        public IValidationBuilder<T> Matches(string pattern, string? message = null)
        {
            if (_currentValue is string str)
            {
                try
                {
                    if (!Regex.IsMatch(str, pattern))
                    {
                        _result.AddError(message ?? $"{_currentField} is not in the correct format", _currentField);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Regex pattern exception: {ex}");
                    _result.AddError($"Invalid pattern: {ex.Message}", _currentField);
                }
            }
            else
            {
                _result.AddError(message ?? $"{_currentField} must be a string for pattern matching", _currentField);
            }
            return this;
        }

        public IValidationBuilder<T> MaxLength(int length, string? message = null)
        {
            if (_currentValue is string str)
            {
                if (str.Length > length)
                {
                    _result.AddError(message ?? $"{_currentField} must have at most {length} characters", _currentField);
                }
            }
            else
            {
                _result.AddError(message ?? $"{_currentField} must be a string for max length validation", _currentField);
            }
            return this;
        }

        public IValidationBuilder<T> MinLength(int length, string? message = null)
        {
            if (_currentValue is string str)
            {
                if (str.Length < length)
                {
                    _result.AddError(message ?? $"{_currentField} must have at least {length} characters", _currentField);
                }
            }
            else
            {
                _result.AddError(message ?? $"{_currentField} must be a string for min length validation", _currentField);
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
            if (_currentValue is TRange value)
            {
                if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                {
                    _result.AddError(message ?? $"{_currentField} must be between {min} and {max}", _currentField);
                }
            }
            else
            {
                _result.AddError(message ?? $"{_currentField} must be comparable for range validation", _currentField);
            }
            return this;
        }

        public IValidationResult Validate() => _result;

        public IValidationBuilder<T> WithField(string fieldName, object? value = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
            _currentField = fieldName;
            _currentValue = value;
            return this;
        }

        public IValidationBuilder<T> WithMetadata<TMetadata>(string key, TMetadata value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            _result.Metadata[key] = value!;
            return this;
        }
    }
}
