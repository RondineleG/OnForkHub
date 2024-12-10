namespace OnForkHub.Api.Endpoints.Base;

public abstract class BaseEndpoint<TEntity>
    where TEntity : BaseEntity
{
    private static readonly Action<ILogger, string, string, Exception?> LogExecutingUseCase = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(1, nameof(LogExecutingUseCase)),
        "Executing usecase {UseCaseName} with request {Request}"
    );

    private static readonly Action<ILogger, string, Exception?> LogOperationCancelled = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(2, nameof(LogOperationCancelled)),
        "Operation cancelled for usecase {UseCaseName}"
    );

    private static readonly Action<ILogger, string, string, Exception?> LogUseCaseError = LoggerMessage.Define<string, string>(
        LogLevel.Error,
        new EventId(3, nameof(LogUseCaseError)),
        "Error executing usecase {UseCaseName}: {ErrorMessage}"
    );

    private static readonly Action<ILogger, string, string, Exception?> LogCreatingResource = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(4, nameof(LogCreatingResource)),
        "Creating resource using {UseCaseName} with request {Request}"
    );

    protected static string GetVersionedRoute(int version, string? customRoute = null)
    {
        var route = customRoute ?? typeof(TEntity).Name.ToLowerInvariant();
        return $"/api/v{version}/{route}";
    }

    protected static ApiVersionSet CreateApiVersionSet(WebApplication app, int version)
    {
        return app.NewApiVersionSet().HasApiVersion(new ApiVersion(version)).ReportApiVersions().Build();
    }

    protected static async Task<IResult> HandleUseCase<TRequest, TResponse>(IUseCase<TRequest, TResponse> useCase, ILogger logger, TRequest request)
    {
        ArgumentNullException.ThrowIfNull(useCase);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            LogExecutingUseCase(logger, useCase.GetType().Name, JsonSerializer.Serialize(request), null);

            var result = await useCase.ExecuteAsync(request);
            return MapResponse(result);
        }
        catch (OperationCanceledException)
        {
            LogOperationCancelled(logger, useCase.GetType().Name, null);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            LogUseCaseError(logger, useCase.GetType().Name, ex.Message, ex);
            return Results.Problem(title: "Internal Server Error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    protected static async Task<IResult> HandleCreateUseCase<TRequest, TResponse>(
        IUseCase<TRequest, TResponse> useCase,
        ILogger logger,
        TRequest request
    )
    {
        ArgumentNullException.ThrowIfNull(useCase);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            LogCreatingResource(logger, useCase.GetType().Name, JsonSerializer.Serialize(request), null);

            var result = await useCase.ExecuteAsync(request);

            if (result.Status == EResultStatus.Success && result is RequestResult<TResponse> typedResult)
            {
                var response = new { data = typedResult.Data, message = result.Message, date = result.Date, id = result.Id };

                var resourceId = typedResult.Data?.GetType().GetProperty("Id")?.GetValue(typedResult.Data)?.ToString();
                var route = $"/{typeof(TEntity).Name.ToLowerInvariant()}/{resourceId ?? result.Id}";
                return Results.Created(route, response);
            }

            return MapResponse(result);
        }
        catch (Exception ex)
        {
            LogUseCaseError(logger, useCase.GetType().Name, ex.Message, ex);
            return Results.Problem(title: "Creation Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    protected static async Task<IResult> HandleDeleteUseCase<TRequest>(IUseCase<TRequest, bool> useCase, ILogger logger, TRequest request)
    {
        ArgumentNullException.ThrowIfNull(useCase);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            LogExecutingUseCase(logger, useCase.GetType().Name, JsonSerializer.Serialize(request), null);

            var result = await useCase.ExecuteAsync(request);

            return result.Status == EResultStatus.Success ? Results.NoContent() : MapResponse(result);
        }
        catch (Exception ex)
        {
            LogUseCaseError(logger, useCase.GetType().Name, ex.Message, ex);
            return Results.Problem(title: "Deletion Failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    protected static RouteHandlerBuilder ConfigureEndpoint(RouteHandlerBuilder builder)
    {
        return builder
            .WithTags(typeof(TEntity).Name)
            .Produces<ValidationProblemDetails>()
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status499ClientClosedRequest);
    }

    private static IResult MapResponse<T>(RequestResult<T> result)
    {
        var response = new
        {
            data = result.Data,
            errors = result.GeneralErrors,
            entityErrors = result.EntityErrors,
            validationErrors = result.ValidationResult?.Errors,
            message = result.Message,
            date = result.Date,
            id = result.Id
        };

        return result.Status switch
        {
            EResultStatus.Success => Results.Ok(response),
            EResultStatus.NoContent => Results.NoContent(),
            EResultStatus.EntityNotFound => Results.NotFound(response),
            EResultStatus.HasValidation => Results.UnprocessableEntity(response),
            EResultStatus.EntityAlreadyExists => Results.Conflict(response),
            EResultStatus.HasError or EResultStatus.EntityHasError => Results.BadRequest(response),
            _ => Results.BadRequest(response)
        };
    }
}