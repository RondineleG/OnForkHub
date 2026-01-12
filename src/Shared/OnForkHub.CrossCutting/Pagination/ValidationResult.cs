namespace OnForkHub.CrossCutting.Pagination;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets a successful validation result.
    /// </summary>
    public static ValidationResult Success => new(true);

    private ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets a value indicating whether the validation passed.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Creates a failed validation result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the validation failure.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);
}
