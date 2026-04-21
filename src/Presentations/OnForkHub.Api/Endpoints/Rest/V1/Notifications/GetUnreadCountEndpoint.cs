namespace OnForkHub.Api.Endpoints.Rest.V1.Notifications;

using OnForkHub.Core.ValueObjects;
using OnForkHub.CrossCutting.Interfaces;

using System.Security.Claims;

/// <summary>
/// Endpoint for getting the count of unread notifications for a user.
/// </summary>
public class GetUnreadCountEndpoint(ILogger<GetUnreadCountEndpoint> logger, INotificationService notificationService)
    : BaseEndPoint<Notification>,
        IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/unread-count";

    private readonly ILogger<GetUnreadCountEndpoint> _logger = logger;

    private readonly INotificationService _notificationService = notificationService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapGet(
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
                            var count = await _notificationService.GetUnreadCountAsync(userIdValue);
                            return Results.Ok(new { count });
                        }
                        catch (Exception ex)
                        {
                            EndpointLogMessages.LogNotificationError(_logger, "GetUnreadCount", ex.Message, ex);
                            return Results.Problem(title: "Error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                        }
                    }
                )
            )
            .WithName("GetUnreadCountV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Gets the count of unread notifications for a user")
            .WithSummary("Get unread notification count")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<object>(StatusCodes.Status200OK)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
