namespace OnForkHub.Application.Test.Dtos;

using OnForkHub.Core.Responses;

public class VideoResponseTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromVideo should throw ArgumentNullException when video is null")]
    public void FromVideoShouldThrowArgumentNullExceptionWhenVideoIsNull()
    {
        Video? video = null;

        var act = () => VideoResponse.FromVideo(video!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromVideo should map all properties correctly")]
    public void FromVideoShouldMapAllPropertiesCorrectly()
    {
        var userId = Id.Create();
        var video = Video.Create("Test Video", "Test Description", "https://example.com/video.mp4", userId).Data!;

        var response = VideoResponse.FromVideo(video);

        response.Title.Should().Be("Test Video");
        response.Description.Should().Be("Test Description");
        response.Url.Should().Be("https://example.com/video.mp4");
        response.OwnerId.Should().Be(userId.ToString());
        response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Default constructor should initialize properties to defaults")]
    public void DefaultConstructorShouldInitializePropertiesToDefaults()
    {
        var response = new VideoResponse();

        response.Id.Should().Be(string.Empty);
        response.Title.Should().Be(string.Empty);
        response.Description.Should().Be(string.Empty);
        response.Url.Should().Be(string.Empty);
        response.OwnerId.Should().Be(string.Empty);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set all properties correctly")]
    public void ShouldSetAllPropertiesCorrectly()
    {
        var response = new VideoResponse
        {
            Id = "video-123",
            Title = "My Video",
            Description = "A great video",
            Url = "https://example.com/video.mp4",
            OwnerId = "user-456",
            CreatedAt = DateTime.UtcNow,
        };

        response.Id.Should().Be("video-123");
        response.Title.Should().Be("My Video");
        response.Description.Should().Be("A great video");
        response.Url.Should().Be("https://example.com/video.mp4");
        response.OwnerId.Should().Be("user-456");
    }
}
