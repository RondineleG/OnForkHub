namespace OnForkHub.Api.Endpoints.Rest.V1.Notifications;

using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for deleting a notification.
/// </summary>
public class DeleteEndpoint(ILogger<DeleteEndpoint> logger, INotificationService notificationService) : BaseEndPoint<Notification>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id}";

    private readonly ILogger<DeleteEndpoint> _logger = logger;

    private readonly INotificationService _notificationService = notificationService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapDelete(
                    Route,
                    async ([FromRoute] string id, CancellationToken cancellationToken = default) =>
                    {
                        try
                        {
                            var result = await _notificationService.DeleteAsync(id);
                            return result.Status == EResultStatus.Success ? Results.NoContent() : MapToResult(result);
                        }
                        catch (Exception ex)
                        {
                            EndpointLogMessages.LogNotificationError(_logger, $"Delete:{id}", ex.Message, ex);
                            return Results.Problem(title: "Error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                        }
                    }
                )
            )
            .WithName("DeleteNotificationV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Deletes a notification")
            .WithSummary("Delete notification")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status204NoContent)
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
