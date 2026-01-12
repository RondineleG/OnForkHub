namespace OnForkHub.CrossCutting.Logging.Implementations;

using System.Text.Json;

/// <summary>
/// In-memory implementation of error logger for development and testing.
/// </summary>
public class InMemoryErrorLogger : IErrorLogger
{
    private readonly List<ErrorLogEntry> _errorLogs = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Logs an exception with correlation ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<string> LogExceptionAsync(Exception exception, string? context = null, string? correlationId = null)
    {
        var errorId = Guid.NewGuid().ToString();

        var logEntry = new ErrorLogEntry
        {
            ErrorId = errorId,
            Message = exception.Message,
            ErrorCode = "EXCEPTION",
            ExceptionType = exception.GetType().Name,
            StackTrace = exception.StackTrace ?? string.Empty,
            CorrelationId = correlationId,
            Context = context,
            Timestamp = DateTime.UtcNow,
        };

        lock (_lockObject)
        {
            _errorLogs.Add(logEntry);
        }

        return await Task.FromResult(errorId);
    }

    /// <summary>
    /// Logs a validation error with detailed information.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<string> LogValidationErrorAsync(Dictionary<string, List<string>> errors, string? userId = null, string? correlationId = null)
    {
        var errorId = Guid.NewGuid().ToString();

        var logEntry = new ErrorLogEntry
        {
            ErrorId = errorId,
            Message = "Validation error occurred",
            ErrorCode = "VALIDATION_ERROR",
            ExceptionType = "ValidationException",
            ValidationErrors = errors,
            UserId = userId,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
        };

        lock (_lockObject)
        {
            _errorLogs.Add(logEntry);
        }

        return await Task.FromResult(errorId);
    }

    /// <summary>
    /// Logs a business logic error.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<string> LogBusinessErrorAsync(
        string errorMessage,
        string errorCode,
        object? additionalData = null,
        string? correlationId = null
    )
    {
        var errorId = Guid.NewGuid().ToString();

        var logEntry = new ErrorLogEntry
        {
            ErrorId = errorId,
            Message = errorMessage,
            ErrorCode = errorCode,
            ExceptionType = "BusinessException",
            AdditionalData = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
        };

        lock (_lockObject)
        {
            _errorLogs.Add(logEntry);
        }

        return await Task.FromResult(errorId);
    }

    /// <summary>
    /// Retrieves logged error information by error ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<ErrorLogEntry?> GetErrorLogAsync(string errorId)
    {
        return Task.Run(() =>
        {
            lock (_lockObject)
            {
                return _errorLogs.FirstOrDefault(x => x.ErrorId == errorId);
            }
        });
    }

    /// <summary>
    /// Retrieves error logs filtered by criteria.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<List<ErrorLogEntry>> GetErrorLogsAsync(
        string? correlationId = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int take = 100
    )
    {
        return Task.Run(() =>
        {
            lock (_lockObject)
            {
                var query = _errorLogs.AsEnumerable();

                if (!string.IsNullOrEmpty(correlationId))
                {
                    query = query.Where(x => x.CorrelationId == correlationId);
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(x => x.UserId == userId);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.Timestamp <= toDate.Value);
                }

                return query.OrderByDescending(x => x.Timestamp).Take(take).ToList();
            }
        });
    }
}
