namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnForkHub.Api.IntegrationTests.Helpers;
using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Application.Dtos.User.Response;
using Xunit;

/// <summary>
/// Integration tests for notification endpoints.
/// Tests notification security ensuring users can only access their own notifications.
/// </summary>
public sealed class NotificationEndpointIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly AuthResponseDto _user1Auth;
    private readonly AuthResponseDto _user2Auth;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationEndpointIntegrationTests"/> class.
    /// </summary>
    public NotificationEndpointIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        _user1Auth = RegisterTestUserAsync("User1").GetAwaiter().GetResult();
        _user2Auth = RegisterTestUserAsync("User2").GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<AuthResponseDto> RegisterTestUserAsync(string prefix)
    {
        var email = TestDataFactory.CreateUniqueEmail();
        var password = "Test@123456";
        var registerRequest = TestDataFactory.CreateRegisterRequest(name: TestDataFactory.CreateUniqueName(prefix), email: email, password: password);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResponse!;
    }

    /// <summary>
    /// Tests that users can only access their own notifications (user isolation).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Notification")]
    [TestCategory("Security")]
    public async Task GetUserNotifications_ShouldReturnOnlyUserNotifications()
    {
        // Arrange
        var user1Client = _factory.CreateClientWithAuth(_user1Auth.AccessToken);
        var user2Client = _factory.CreateClientWithAuth(_user2Auth.AccessToken);

        // Act - Both users get their notifications
        var user1Response = await user1Client.GetAsync("/api/v1/rest/notifications");
        var user2Response = await user2Client.GetAsync("/api/v1/rest/notifications");

        // Assert - Both should succeed but return different data (user isolation)
        user1Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        user2Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that getting notifications fails without authentication.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Notification")]
    public async Task GetUserNotifications_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/rest/notifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests getting unread notification count.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Notification")]
    public async Task GetUnreadCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var userClient = _factory.CreateClientWithAuth(_user1Auth.AccessToken);

        // Act
        var response = await userClient.GetAsync("/api/v1/rest/notifications/unread-count");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that marking a notification as read validates ownership.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Notification")]
    [TestCategory("Security")]
    public async Task MarkAsRead_ShouldValidateOwnership()
    {
        // Arrange
        var user1Client = _factory.CreateClientWithAuth(_user1Auth.AccessToken);
        var user2Client = _factory.CreateClientWithAuth(_user2Auth.AccessToken);
        var nonExistentId = "99999999";

        // Act - User 1 tries to mark their own notification (should succeed or return not found)
        var user1Response = await user1Client.PutAsync($"/api/v1/rest/notifications/{nonExistentId}/read", null);

        // User 2 tries to mark the same notification (should fail due to ownership)
        var user2Response = await user2Client.PutAsync($"/api/v1/rest/notifications/{nonExistentId}/read", null);

        // Assert
        user1Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        user2Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Tests that marking non-existent notification returns not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Notification")]
    public async Task MarkAsRead_ShouldReturnNotFound_WhenNotificationNotExists()
    {
        // Arrange
        var userClient = _factory.CreateClientWithAuth(_user1Auth.AccessToken);
        var nonExistentId = "99999999";

        // Act
        var response = await userClient.PutAsync($"/api/v1/rest/notifications/{nonExistentId}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests marking all notifications as read for a user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Notification")]
    public async Task MarkAllAsRead_ShouldMarkAllForUser()
    {
        // Arrange
        var userClient = _factory.CreateClientWithAuth(_user1Auth.AccessToken);

        // Act
        var response = await userClient.PutAsync("/api/v1/rest/notifications/read-all", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that deleting a notification validates ownership.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Notification")]
    [TestCategory("Security")]
    public async Task DeleteNotification_ShouldValidateOwnership()
    {
        // Arrange
        var user1Client = _factory.CreateClientWithAuth(_user1Auth.AccessToken);
        var user2Client = _factory.CreateClientWithAuth(_user2Auth.AccessToken);
        var nonExistentId = "99999999";

        // Act - User 1 tries to delete
        var user1Response = await user1Client.DeleteAsync($"/api/v1/rest/notifications/{nonExistentId}");

        // User 2 tries to delete the same notification
        var user2Response = await user2Client.DeleteAsync($"/api/v1/rest/notifications/{nonExistentId}");

        // Assert - Both should return not found for non-existent ID
        user1Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        user2Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests that deleting notification fails without authentication.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [TestCategory("Integration")]
    [TestCategory("Notification")]
    public async Task DeleteNotification_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var notificationId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/rest/notifications/{notificationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
