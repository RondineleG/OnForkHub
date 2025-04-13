namespace OnForkHub.Api.Endpoints.Base;

public abstract class BaseEndpoint<TEntity>
    where TEntity : BaseEntity
{
    protected static RouteHandlerBuilder ConfigureEndpoint(RouteHandlerBuilder builder)
    {
        return builder
            .WithTags(typeof(TEntity).Name)
            .Produces<ValidationProblemDetails>()
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status499ClientClosedRequest);
    }

    protected static ApiVersionSet CreateApiVersionSet(WebApplication app, int version)
    {
        return app.NewApiVersionSet().HasApiVersion(new ApiVersion(version)).ReportApiVersions().Build();
    }

    protected static string GetVersionedRoute(int version, string? customRoute = null)
    {
        var route = customRoute ?? typeof(TEntity).Name.ToLowerInvariant();
        return $"/api/v{version}/{route}";
    }

    protected static async Task<IResult> HandleUseCaseAsync<TRequest, TResponse>(
        IUseCase<TRequest, TResponse> useCase,
        ILogger logger,
        TRequest request,
        Func<RequestResult<TResponse>, IResult> successHandler,
        string errorTitle
    )
    {
        ArgumentNullException.ThrowIfNull(useCase);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            EndpointLogMessages.LogExecutingUseCase(logger, useCase.GetType().Name, JsonSerializer.Serialize(request), null);
            var result = await useCase.ExecuteAsync(request);

            return result.Status == EResultStatus.Success ? successHandler(result) : MapResponse(result);
        }
        catch (OperationCanceledException)
        {
            EndpointLogMessages.LogOperationCancelled(logger, useCase.GetType().Name, null);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            EndpointLogMessages.LogUseCaseError(logger, useCase.GetType().Name, ex.Message, ex);
            return Results.Problem(title: errorTitle, detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    protected static async Task<IResult> HandleCreateUseCase<TRequest, TResponse>(
        IUseCase<TRequest, TResponse> useCase,
        ILogger logger,
        TRequest request
    )
    {
        return await HandleUseCaseAsync(
            useCase,
            logger,
            request,
            result =>
            {
                var response = new
                {
                    data = result.Data,
                    message = result.Message,
                    date = result.Date,
                    id = result.Id,
                };

                var resourceId = result.Data?.GetType().GetProperty("Id")?.GetValue(result.Data)?.ToString();
                var route = $"/{typeof(TEntity).Name.ToLowerInvariant()}/{resourceId ?? result.Id}";
                return Results.Created(route, response);
            },
            "Creation Failed"
        );
    }

    protected static async Task<IResult> HandleDeleteUseCase<TRequest, TResponse>(
        IUseCase<TRequest, TResponse> useCase,
        ILogger logger,
        TRequest request
    )
    {
        return await HandleUseCaseAsync(useCase, logger, request, _ => Results.NoContent(), "Deletion Failed");
    }

    protected static async Task<IResult> HandleUseCase<TRequest, TResponse>(IUseCase<TRequest, TResponse> useCase, ILogger logger, TRequest request)
    {
        return await HandleUseCaseAsync(useCase, logger, request, MapResponse, "Internal Server Error");
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
            id = result.Id,
        };

        return result.Status switch
        {
            EResultStatus.Success => Results.Ok(response),
            EResultStatus.NoContent => Results.NoContent(),
            EResultStatus.EntityNotFound => Results.NotFound(response),
            EResultStatus.HasValidation => Results.UnprocessableEntity(response),
            EResultStatus.EntityAlreadyExists => Results.Conflict(response),
            EResultStatus.HasError or EResultStatus.EntityHasError => Results.BadRequest(response),
            _ => Results.BadRequest(response),
        };
    }
}