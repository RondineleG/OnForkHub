namespace OnForkHub.Core.Validations;

public sealed class ValidationErrorMessage
{
    public ValidationErrorMessage(string message, string field = "")
    {
        Message = message;
        Field = field;
    }

    public ValidationErrorMessage(string message, string field, string? source = null)
    {
        Message = message;
        Field = field;
        Source = source ?? string.Empty;
        Timestamp = DateTime.UtcNow;
    }

    public string Source { get; } = string.Empty;
    public string Field { get; } = string.Empty;

    public string Message { get; } = string.Empty;

    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public override string ToString()
    {
        return JsonSerializer.Serialize(
            new
            {
                Field,
                Message,
                Timestamp,
                Source,
            }
        );
    }
}
