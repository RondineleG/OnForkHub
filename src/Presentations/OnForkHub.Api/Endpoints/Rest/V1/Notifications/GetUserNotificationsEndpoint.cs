namespace OnForkHub.Api.Endpoints.Rest.V1.Notifications;

using OnForkHub.Core.Interfaces.Configuration;
using OnForkHub.Core.ValueObjects;

/// <summary>
/// Endpoint for getting user notifications with pagination.
/// </summary>
public class GetUserNotificationsEndpoint(ILogger<GetUserNotificationsEndpoint> logger, INotificationService notificationService)
    : BaseEndPoint<Notification>,
        IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1);

    private readonly ILogger<GetUserNotificationsEndpoint> _logger = logger;

    private readonly INotificationService _notificationService = notificationService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async (
                        [FromQuery] string userId,
                        [FromQuery] int page = 1,
                        [FromQuery] int pageSize = 10,
                        CancellationToken cancellationToken = default
                    ) =>
                    {
                        try
                        {
                            Id userIdValue = userId;
                            var result = await _notificationService.GetUserNotificationsAsync(userIdValue, page, pageSize);
                            return MapToResult(result);
                        }
                        catch (Exception ex)
                        {
                            EndpointLogMessages.LogNotificationError(_logger, "GetUserNotifications", ex.Message, ex);
                            return Results.Problem(title: "Error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                        }
                    }
                )
            )
            .WithName("GetUserNotificationsV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Gets user notifications with pagination")
            .WithSummary("Get user notifications")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<IEnumerable<Notification>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
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
