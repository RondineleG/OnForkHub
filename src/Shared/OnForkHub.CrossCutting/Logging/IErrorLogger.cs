namespace OnForkHub.CrossCutting.Logging;

/// <summary>
/// Defines contract for centralized error logging service.
/// </summary>
public interface IErrorLogger
{
    /// <summary>
    /// Logs an exception with correlation ID.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="context">Optional context information.</param>
    /// <param name="correlationId">The correlation ID for tracking.</param>
    /// <returns>A unique error ID for reference.</returns>
    Task<string> LogExceptionAsync(Exception exception, string? context = null, string? correlationId = null);

    /// <summary>
    /// Logs a validation error with detailed information.
    /// </summary>
    /// <param name="errors">Dictionary of property names and their error messages.</param>
    /// <param name="userId">Optional user ID for audit trail.</param>
    /// <param name="correlationId">The correlation ID for tracking.</param>
    /// <returns>A unique error ID for reference.</returns>
    Task<string> LogValidationErrorAsync(Dictionary<string, List<string>> errors, string? userId = null, string? correlationId = null);

    /// <summary>
    /// Logs a business logic error.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="additionalData">Optional additional context data.</param>
    /// <param name="correlationId">The correlation ID for tracking.</param>
    /// <returns>A unique error ID for reference.</returns>
    Task<string> LogBusinessErrorAsync(string errorMessage, string errorCode, object? additionalData = null, string? correlationId = null);

    /// <summary>
    /// Retrieves logged error information by error ID.
    /// </summary>
    /// <param name="errorId">The unique error ID.</param>
    /// <returns>The error log entry or null if not found.</returns>
    Task<ErrorLogEntry?> GetErrorLogAsync(string errorId);

    /// <summary>
    /// Retrieves error logs filtered by criteria.
    /// </summary>
    /// <param name="correlationId">Optional correlation ID filter.</param>
    /// <param name="userId">Optional user ID filter.</param>
    /// <param name="fromDate">Optional start date filter.</param>
    /// <param name="toDate">Optional end date filter.</param>
    /// <param name="take">Number of records to retrieve.</param>
    /// <returns>A list of error log entries matching the criteria.</returns>
    Task<List<ErrorLogEntry>> GetErrorLogsAsync(
        string? correlationId = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int take = 100
    );
}

/// <summary>
/// Represents a logged error entry.
/// </summary>
public class ErrorLogEntry
{
    /// <summary>
    /// Gets or sets the unique error ID.
    /// </summary>
    public string ErrorId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exception type name.
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stack trace.
    /// </summary>
    public string StackTrace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation ID for tracking.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the user ID for audit trail.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the context information.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Gets or sets validation errors if applicable.
    /// </summary>
    public Dictionary<string, List<string>>? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets additional data.
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets whether error has been resolved.
    /// </summary>
    public bool IsResolved { get; set; }
}
