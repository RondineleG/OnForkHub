namespace OnForkHub.Api.Endpoints.Rest.V1.Notifications;

using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for getting a notification by ID.
/// </summary>
public class GetByIdEndpoint(ILogger<GetByIdEndpoint> logger, INotificationService notificationService) : BaseEndPoint<Notification>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id}";

    private readonly ILogger<GetByIdEndpoint> _logger = logger;

    private readonly INotificationService _notificationService = notificationService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async ([FromRoute] string id, CancellationToken cancellationToken = default) =>
                    {
                        try
                        {
                            var result = await _notificationService.GetByIdAsync(id);
                            return MapToResult(result);
                        }
                        catch (Exception ex)
                        {
                            EndpointLogMessages.LogNotificationError(_logger, $"GetById:{id}", ex.Message, ex);
                            return Results.Problem(title: "Error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                        }
                    }
                )
            )
            .WithName("GetNotificationByIdV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Gets a notification by ID")
            .WithSummary("Get notification by ID")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<Notification>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }

    private static IResult MapToResult<T>(RequestResult<T> result)
    {
        return result.Status switch
        {
            EResultStatus.Success => Results.Ok(result),
            EResultStatus.NoContent => Results.NoContent(),
            EResultStatus.EntityNotFound => Results.NotFound(result),
            EResultStatus.HasError or EResultStatus.EntityHasError => Results.BadRequest(result),
            _ => Results.BadRequest(result),
        };
    }
}
