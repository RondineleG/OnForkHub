namespace OnForkHub.Api.Services;

using Microsoft.AspNetCore.SignalR;

using OnForkHub.Api.Hubs;
using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service for sending real-time notifications via SignalR.
/// </summary>
public sealed class RealTimeNotificationService(IHubContext<NotificationHub> hubContext)
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;

    /// <summary>
    /// Sends a notification to a specific user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendNotificationAsync(string userId, string message, string type)
    {
        await _hubContext
            .Clients.Group(userId)
            .SendAsync(
                "ReceiveNotification",
                new
                {
                    message,
                    type,
                    createdAt = DateTime.UtcNow,
                }
            );
    }
}
