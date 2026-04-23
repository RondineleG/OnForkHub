namespace OnForkHub.Web.Services.RealTime;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Client for receiving real-time notifications via SignalR.
/// </summary>
public sealed class NotificationClient : IAsyncDisposable
{
    private readonly NavigationManager _navigationManager;
    private HubConnection? _hubConnection;
    private bool _started;

    public NotificationClient(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Event triggered when a notification is received.
    /// </summary>
    public event Action<string, string, DateTime>? OnNotificationReceived;

    /// <summary>
    /// Starts the SignalR connection.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartAsync(string accessToken)
    {
        if (_started)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(
                _navigationManager.ToAbsoluteUri("/hubs/notifications"),
                options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
                }
            )
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<NotificationPayload>(
            "ReceiveNotification",
            (payload) =>
            {
                OnNotificationReceived?.Invoke(payload.Message, payload.Type, payload.CreatedAt);
            }
        );

        await _hubConnection.StartAsync();
        _started = true;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    private sealed record NotificationPayload(string Message, string Type, DateTime CreatedAt);
}
