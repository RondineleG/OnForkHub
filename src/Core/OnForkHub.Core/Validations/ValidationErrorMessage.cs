namespace OnForkHub.Core.Validations;

public sealed class ValidationErrorMessage
{
    public ValidationErrorMessage(string message, string field = "")
    {
        Message = message;
        Field = field;
        ErrorCode = EErrorCode.ValidationFailed;
    }

    public ValidationErrorMessage(string message, string field, EErrorCode errorCode)
    {
        Message = message;
        Field = field;
        ErrorCode = errorCode;
        Source = string.Empty;
        Timestamp = DateTime.UtcNow;
    }

    public ValidationErrorMessage(string message, string field, string? source = null)
    {
        Message = message;
        Field = field;
        ErrorCode = EErrorCode.ValidationFailed;
        Source = source ?? string.Empty;
        Timestamp = DateTime.UtcNow;
    }

    public ValidationErrorMessage(string message, string field, EErrorCode errorCode, string? source = null)
    {
        Message = message;
        Field = field;
        ErrorCode = errorCode;
        Source = source ?? string.Empty;
        Timestamp = DateTime.UtcNow;
    }

    public EErrorCode ErrorCode { get; }

    public string Field { get; }

    public string Message { get; }

    public string Source { get; } = string.Empty;

    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public override string ToString()
    {
        return JsonSerializer.Serialize(
            new
            {
                Field,
                Message,
                ErrorCode,
                Timestamp,
                Source,
            }
        );
    }
}
