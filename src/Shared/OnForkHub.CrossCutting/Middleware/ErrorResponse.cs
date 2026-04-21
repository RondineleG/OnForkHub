namespace OnForkHub.CrossCutting.Middleware;

/// <summary>
/// Standardized API error response model.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets hTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets application-specific error code.
    /// </summary>
    public EErrorCode ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets user-friendly error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional error details or validation errors.
    /// </summary>
    public List<string>? Details { get; set; }

    /// <summary>
    /// Gets or sets structured validation errors grouped by property.
    /// </summary>
    public Dictionary<string, List<string>>? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets timestamp of when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets trace ID for logging correlation.
    /// </summary>
    public string? TraceId { get; set; }
}
