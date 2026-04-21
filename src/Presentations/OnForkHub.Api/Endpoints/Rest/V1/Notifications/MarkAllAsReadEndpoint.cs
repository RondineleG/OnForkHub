namespace OnForkHub.Api.Endpoints.Rest.V1.Notifications;

using System.Security.Claims;
using OnForkHub.Core.ValueObjects;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for marking all notifications as read for a user.
/// </summary>
public class MarkAllAsReadEndpoint(ILogger<MarkAllAsReadEndpoint> logger, INotificationService notificationService)
    : BaseEndPoint<Notification>,
        IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/read-all";

    private readonly ILogger<MarkAllAsReadEndpoint> _logger = logger;

    private readonly INotificationService _notificationService = notificationService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapPut(
                    Route,
                    async (HttpContext httpContext, CancellationToken cancellationToken = default) =>
                    {
                        // SECURITY: Extract userId from JWT Claims instead of query parameter
                        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                        if (string.IsNullOrEmpty(userId))
                        {
                            return Results.Unauthorized();
                        }

                        try
                        {
                            Id userIdValue = userId;
                            var result = await _notificationService.MarkAllAsReadAsync(userIdValue);
                            return MapToResult(result);
                        }
                        catch (Exception ex)
                        {
                            EndpointLogMessages.LogNotificationError(_logger, "MarkAllAsRead", ex.Message, ex);
                            return Results.Problem(title: "Error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                        }
                    }
                )
            )
            .WithName("MarkAllNotificationsAsReadV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Marks all notifications as read for a user")
            .WithSummary("Mark all notifications as read")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<int>>(StatusCodes.Status200OK)
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
