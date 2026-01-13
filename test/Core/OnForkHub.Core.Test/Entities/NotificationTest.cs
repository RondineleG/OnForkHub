namespace OnForkHub.Core.Test.Entities;

public class NotificationTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should successfully create notification with valid data")]
    public void ShouldSuccessfullyCreateNotificationWithValidData()
    {
        var title = "Test Notification";
        var message = "This is a test notification message";
        var userId = Id.Create();
        var type = ENotificationType.System;

        var result = Notification.Create(title, message, userId, type);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Title.Value.Should().Be(title);
        result.Data.Message.Should().Be(message);
        result.Data.UserId.Should().Be(userId);
        result.Data.Type.Should().Be(type);
        result.Data.Status.Should().Be(ENotificationStatus.Unread);
        result.Data.ReadAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create notification with reference ID")]
    public void ShouldCreateNotificationWithReferenceId()
    {
        var title = "Video Upload Complete";
        var message = "Your video has been processed";
        var userId = Id.Create();
        var type = ENotificationType.VideoProcessed;
        var referenceId = "video-123";

        var result = Notification.Create(title, message, userId, type, referenceId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data!.ReferenceId.Should().Be(referenceId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should mark notification as read")]
    public void ShouldMarkNotificationAsRead()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;

        var result = notification.MarkAsRead();

        result.Status.Should().Be(EResultStatus.Success);
        notification.Status.Should().Be(ENotificationStatus.Read);
        notification.ReadAt.Should().NotBeNull();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should mark notification as unread")]
    public void ShouldMarkNotificationAsUnread()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;
        notification.MarkAsRead();

        var result = notification.MarkAsUnread();

        result.Status.Should().Be(EResultStatus.Success);
        notification.Status.Should().Be(ENotificationStatus.Unread);
        notification.ReadAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should archive notification")]
    public void ShouldArchiveNotification()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;

        var result = notification.Archive();

        result.Status.Should().Be(EResultStatus.Success);
        notification.Status.Should().Be(ENotificationStatus.Archived);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should delete notification")]
    public void ShouldDeleteNotification()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;

        var result = notification.Delete();

        result.Status.Should().Be(EResultStatus.Success);
        notification.Status.Should().Be(ENotificationStatus.Deleted);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return success when marking already read notification as read")]
    public void ShouldReturnSuccessWhenMarkingAlreadyReadNotificationAsRead()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;
        notification.MarkAsRead();

        var result = notification.MarkAsRead();

        result.Status.Should().Be(EResultStatus.Success);
        notification.Status.Should().Be(ENotificationStatus.Read);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return success when marking already unread notification as unread")]
    public void ShouldReturnSuccessWhenMarkingAlreadyUnreadNotificationAsUnread()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;

        var result = notification.MarkAsUnread();

        result.Status.Should().Be(EResultStatus.Success);
        notification.Status.Should().Be(ENotificationStatus.Unread);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when creating notification with empty title")]
    public void ShouldReturnErrorWhenCreatingNotificationWithEmptyTitle()
    {
        var result = Notification.Create(string.Empty, "Message", Id.Create(), ENotificationType.System);

        result.Status.Should().Be(EResultStatus.HasError);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when creating notification with empty message")]
    public void ShouldReturnErrorWhenCreatingNotificationWithEmptyMessage()
    {
        var result = Notification.Create("Title", string.Empty, Id.Create(), ENotificationType.System);

        result.Status.Should().Be(EResultStatus.HasError);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when creating notification with null userId")]
    public void ShouldReturnErrorWhenCreatingNotificationWithNullUserId()
    {
        var act = () => Notification.Create("Title", "Message", null!, ENotificationType.System);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when creating notification with message exceeding max length")]
    public void ShouldReturnErrorWhenCreatingNotificationWithMessageExceedingMaxLength()
    {
        var longMessage = new string('a', 1001);

        var result = Notification.Create("Title", longMessage, Id.Create(), ENotificationType.System);

        result.Status.Should().Be(EResultStatus.HasError);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should successfully load notification with valid data")]
    public void ShouldSuccessfullyLoadNotificationWithValidData()
    {
        var id = Id.Create();
        var title = "Test Notification";
        var message = "This is a test notification message";
        var userId = Id.Create();
        var type = ENotificationType.VideoUploaded;
        var status = ENotificationStatus.Read;
        var referenceId = "video-456";
        var readAt = DateTime.UtcNow;
        var createdAt = DateTime.UtcNow.AddHours(-1);
        var updatedAt = DateTime.UtcNow;

        var result = Notification.Load(id, title, message, userId, type, status, referenceId, readAt, createdAt, updatedAt);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Title.Value.Should().Be(title);
        result.Data.Message.Should().Be(message);
        result.Data.UserId.Should().Be(userId);
        result.Data.Type.Should().Be(type);
        result.Data.Status.Should().Be(status);
        result.Data.ReferenceId.Should().Be(referenceId);
        result.Data.ReadAt.Should().Be(readAt);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create notification with different types")]
    public void ShouldCreateNotificationWithDifferentTypes()
    {
        var notificationTypes = new[]
        {
            ENotificationType.System,
            ENotificationType.VideoUploaded,
            ENotificationType.VideoProcessed,
            ENotificationType.VideoDeleted,
            ENotificationType.CategoryCreated,
            ENotificationType.CategoryUpdated,
            ENotificationType.UserRegistered,
            ENotificationType.UserUpdated,
            ENotificationType.SecurityAlert,
            ENotificationType.Welcome,
        };

        foreach (var type in notificationTypes)
        {
            var result = Notification.Create("Title", "Message", Id.Create(), type);

            result.Status.Should().Be(EResultStatus.Success);
            result.Data!.Type.Should().Be(type);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set UpdatedAt when marking as read")]
    public void ShouldSetUpdatedAtWhenMarkingAsRead()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;
        var initialUpdatedAt = notification.UpdatedAt;

        notification.MarkAsRead();

        notification.UpdatedAt.Should().NotBe(initialUpdatedAt);
        notification.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set UpdatedAt when archiving")]
    public void ShouldSetUpdatedAtWhenArchiving()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;
        var initialUpdatedAt = notification.UpdatedAt;

        notification.Archive();

        notification.UpdatedAt.Should().NotBe(initialUpdatedAt);
        notification.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set UpdatedAt when deleting")]
    public void ShouldSetUpdatedAtWhenDeleting()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;
        var initialUpdatedAt = notification.UpdatedAt;

        notification.Delete();

        notification.UpdatedAt.Should().NotBe(initialUpdatedAt);
        notification.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should use correct collection name")]
    public void ShouldUseCorrectCollectionName()
    {
        var notification = Notification.Create("Title", "Message", Id.Create(), ENotificationType.System).Data!;

        notification.Id.Should().StartWith("notifications/");
    }
}
