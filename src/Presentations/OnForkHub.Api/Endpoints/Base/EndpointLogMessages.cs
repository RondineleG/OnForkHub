namespace OnForkHub.Api.Endpoints.Base;

public static class EndpointLogMessages
{
    public static readonly Action<ILogger, string, string, Exception?> LogCreatingResource = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(4, nameof(LogCreatingResource)),
        "Creating resource using {UseCaseName} with request {Request}"
    );

    public static readonly Action<ILogger, string, string, Exception?> LogExecutingUseCase = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(1, nameof(LogExecutingUseCase)),
        "Executing usecase {UseCaseName} with request {Request}"
    );

    public static readonly Action<ILogger, string, Exception?> LogOperationCancelled = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(2, nameof(LogOperationCancelled)),
        "Operation cancelled for usecase {UseCaseName}"
    );

    public static readonly Action<ILogger, string, string, Exception?> LogUseCaseError = LoggerMessage.Define<string, string>(
        LogLevel.Error,
        new EventId(3, nameof(LogUseCaseError)),
        "Error executing usecase {UseCaseName}: {ErrorMessage}"
    );
}