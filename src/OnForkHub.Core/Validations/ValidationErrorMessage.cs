using System.Text.Json;

namespace OnForkHub.Core.Validations;

public sealed class ValidationErrorMessage
{
    public ValidationErrorMessage(string message, string field = "")
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Field = field ?? "";
        Timestamp = DateTime.UtcNow;
    }

    public string Field { get; }

    public string Message { get; }

    public DateTime Timestamp { get; }

    public override string ToString() =>
        JsonSerializer.Serialize(
            new
            {
                Field,
                Message,
                Timestamp,
            }
        );
}
