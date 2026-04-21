namespace OnForkHub.Core.Exceptions;

/// <summary>
/// Represents validation errors with support for multiple field-level errors.
/// </summary>
public class ValidationException : CustomException
{
    private readonly Dictionary<string, List<string>> _errors;

    /// <summary>
    /// Gets the collection of validation errors grouped by property name.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors =>
        _errors.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value.AsReadOnly());

    /// <summary>
    /// Gets a value indicating whether any validation errors exist.
    /// </summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Gets the total number of validation errors.
    /// </summary>
    public int ErrorCount => _errors.Sum(x => x.Value.Count);

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a single error.
    /// </summary>
    /// <param name="propertyName">The name of the property that failed validation.</param>
    /// <param name="errorMessage">The validation error message.</param>
    public ValidationException(string propertyName, string errorMessage)
        : base(BuildMessage(propertyName, errorMessage), "VALIDATION_ERROR")
    {
        _errors = new Dictionary<string, List<string>>
        {
            {
                propertyName,
                new List<string> { errorMessage }
            },
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with multiple errors.
    /// </summary>
    /// <param name="errors">A dictionary of property names and their validation error messages.</param>
    public ValidationException(Dictionary<string, List<string>> errors)
        : base(BuildMessage(errors), "VALIDATION_ERROR")
    {
        _errors = errors ?? new Dictionary<string, List<string>>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a custom message and errors.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errors">A dictionary of property names and their validation error messages.</param>
    public ValidationException(string message, Dictionary<string, List<string>> errors)
        : base(message, "VALIDATION_ERROR")
    {
        _errors = errors ?? new Dictionary<string, List<string>>();
    }

    /// <summary>
    /// Gets the validation errors for a specific property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>A list of error messages for the property, or an empty list if no errors exist.</returns>
    public IReadOnlyList<string> GetErrors(string propertyName)
    {
        return _errors.TryGetValue(propertyName, out var errors) ? errors.AsReadOnly() : new List<string>().AsReadOnly();
    }

    /// <summary>
    /// Gets all validation error messages as a single concatenated string.
    /// </summary>
    /// <returns>All error messages separated by semicolons.</returns>
    public string GetAllErrorsAsString() => string.Join("; ", _errors.SelectMany(x => x.Value.Select(e => $"{x.Key}: {e}")));

    /// <summary>
    /// Adds an error message to a specific property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="errorMessage">The error message.</param>
    public void AddError(string propertyName, string errorMessage)
    {
        if (!_errors.TryGetValue(propertyName, out var errors))
        {
            errors = new List<string>();
            _errors[propertyName] = errors;
        }

        errors.Add(errorMessage);
    }

    /// <summary>
    /// Adds multiple error messages for a specific property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="errorMessages">The error messages to add.</param>
    public void AddErrors(string propertyName, params string[] errorMessages)
    {
        if (!_errors.TryGetValue(propertyName, out var errors))
        {
            errors = new List<string>();
            _errors[propertyName] = errors;
        }

        errors.AddRange(errorMessages);
    }

    private static string BuildMessage(string propertyName, string errorMessage) => $"Validation error: {propertyName} - {errorMessage}";

    private static string BuildMessage(Dictionary<string, List<string>> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            return "Validation failed";
        }

        var errorMessages = errors.SelectMany(x => x.Value.Select(e => $"{x.Key}: {e}"));
        return $"Validation failed: {string.Join("; ", errorMessages)}";
    }
}
