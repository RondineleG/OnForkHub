namespace OnForkHub.CrossCutting.Middleware;

using Microsoft.AspNetCore.Http;

using OnForkHub.Core.Exceptions;

/// <summary>
/// Global exception handling middleware for centralized error response management.
/// Catches unhandled exceptions and returns standardized error responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
#pragma warning disable CA1848
            _logger.LogError(exception, "Unhandled exception occurred");
#pragma warning restore CA1848
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = MapExceptionToErrorResponse(exception);
        response.TraceId = context.TraceIdentifier;

        context.Response.StatusCode = response.StatusCode;

        return context.Response.WriteAsJsonAsync(response);
    }

    private static ErrorResponse MapExceptionToErrorResponse(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = EErrorCode.ValidationFailed,
                Message = validationEx.Message,
                ValidationErrors = validationEx.Errors.ToDictionary(x => x.Key, x => x.Value.ToList()),
                Details = new List<string> { $"Total validation errors: {validationEx.ErrorCount}" },
            },

            DomainException domainEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = EErrorCode.ValidationFailed,
                Message = domainEx.Message,
            },

            ArgumentNullException argNullEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = EErrorCode.PropertyRequired,
                Message = $"Required parameter '{argNullEx.ParamName}' is null",
            },

            ArgumentException argEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = EErrorCode.InvalidFormat,
                Message = argEx.Message,
            },

            _ => new ErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorCode = EErrorCode.InternalServerError,
                Message = "An unexpected error occurred. Please try again later.",
            },
        };
    }
}
