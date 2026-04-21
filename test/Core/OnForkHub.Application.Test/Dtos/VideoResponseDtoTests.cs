namespace OnForkHub.Application.Test.Dtos;

using OnForkHub.Application.Dtos.Video.Response;

public class VideoResponseDtoTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromVideo should throw ArgumentNullException when video is null")]
    public void FromVideoShouldThrowArgumentNullExceptionWhenVideoIsNull()
    {
        Video? video = null;

        var act = () => VideoResponseDto.FromVideo(video!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromVideo should map all properties correctly")]
    public void FromVideoShouldMapAllPropertiesCorrectly()
    {
        var userId = Id.Create();
        var video = Video.Create("Test Video", "Test Description", "https://example.com/video.mp4", userId).Data!;

        var dto = VideoResponseDto.FromVideo(video);

        dto.Title.Should().Be("Test Video");
        dto.Description.Should().Be("Test Description");
        dto.Url.Should().Be("https://example.com/video.mp4");
        dto.UserId.Should().Be(userId.ToString());
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Default constructor should initialize properties to defaults")]
    public void DefaultConstructorShouldInitializePropertiesToDefaults()
    {
        var dto = new VideoResponseDto();

        dto.Id.Should().Be(string.Empty);
        dto.Title.Should().Be(string.Empty);
        dto.Description.Should().Be(string.Empty);
        dto.Url.Should().Be(string.Empty);
        dto.UserId.Should().Be(string.Empty);
        dto.Categories.Should().BeEmpty();
        dto.UpdatedAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set all properties correctly")]
    public void ShouldSetAllPropertiesCorrectly()
    {
        var dto = new VideoResponseDto
        {
            Id = "video-123",
            Title = "My Video",
            Description = "A great video",
            Url = "https://example.com/video.mp4",
            UserId = "user-456",
            Categories = new List<string> { "Category 1", "Category 2" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        dto.Id.Should().Be("video-123");
        dto.Title.Should().Be("My Video");
        dto.Description.Should().Be("A great video");
        dto.Url.Should().Be("https://example.com/video.mp4");
        dto.UserId.Should().Be("user-456");
        dto.Categories.Should().HaveCount(2);
        dto.Categories.Should().Contain("Category 1");
        dto.Categories.Should().Contain("Category 2");
        dto.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromVideo should handle video without categories")]
    public void FromVideoShouldHandleVideoWithoutCategories()
    {
        var video = Video.Create("Test Video", "Test Description", "https://example.com/video.mp4", Id.Create()).Data!;

        var dto = VideoResponseDto.FromVideo(video);

        dto.Categories.Should().BeEmpty();
    }
}
