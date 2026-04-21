namespace OnForkHub.Api.Endpoints.Rest.V1.Notifications;

using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for marking a notification as read.
/// </summary>
public class MarkAsReadEndpoint(ILogger<MarkAsReadEndpoint> logger, INotificationService notificationService)
    : BaseEndPoint<Notification>,
        IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id}/read";

    private readonly ILogger<MarkAsReadEndpoint> _logger = logger;

    private readonly INotificationService _notificationService = notificationService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapPut(
                    Route,
                    async ([FromRoute] string id, CancellationToken cancellationToken = default) =>
                    {
                        try
                        {
                            var result = await _notificationService.MarkAsReadAsync(id);
                            return MapToResult(result);
                        }
                        catch (Exception ex)
                        {
                            EndpointLogMessages.LogNotificationError(_logger, $"MarkAsRead:{id}", ex.Message, ex);
                            return Results.Problem(title: "Error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                        }
                    }
                )
            )
            .WithName("MarkNotificationAsReadV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Marks a notification as read")
            .WithSummary("Mark notification as read")
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
