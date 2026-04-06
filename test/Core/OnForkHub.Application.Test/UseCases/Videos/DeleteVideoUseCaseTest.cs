using OnForkHub.Application.UseCases.Videos;

namespace OnForkHub.Application.Test.UseCases.Videos;

public class DeleteVideoUseCaseTest
{
    private readonly IVideoService _videoService;
    private readonly DeleteVideoUseCase _useCase;

    public DeleteVideoUseCaseTest()
    {
        _videoService = Substitute.For<IVideoService>();
        _useCase = new DeleteVideoUseCase(_videoService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should delete video successfully")]
    public async Task ShouldDeleteVideoSuccessfully()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var video = CreateValidVideo(videoId);

        _videoService.DeleteAsync(Arg.Any<Id>())
            .Returns(RequestResult<Video>.Success(video));

        // Act
        var result = await _useCase.ExecuteAsync(videoId);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        await _videoService.Received(1).DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentException when videoId is null")]
    public async Task ShouldThrowArgumentExceptionWhenVideoIdIsNull()
    {
        // Arrange
        string? videoId = null;

        // Act
        var act = () => _useCase.ExecuteAsync(videoId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        await _videoService.DidNotReceive().DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentException when videoId is empty")]
    public async Task ShouldThrowArgumentExceptionWhenVideoIdIsEmpty()
    {
        // Arrange
        var videoId = string.Empty;

        // Act
        var act = () => _useCase.ExecuteAsync(videoId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        await _videoService.DidNotReceive().DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentException when videoId is whitespace")]
    public async Task ShouldThrowArgumentExceptionWhenVideoIdIsWhitespace()
    {
        // Arrange
        var videoId = "   ";

        // Act
        var act = () => _useCase.ExecuteAsync(videoId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        await _videoService.DidNotReceive().DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when videoId has invalid guid format")]
    public async Task ShouldReturnErrorWhenVideoIdHasInvalidGuidFormat()
    {
        // Arrange
        var videoId = "invalid-guid-format";

        // Act
        var result = await _useCase.ExecuteAsync(videoId);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Invalid video ID format");
        await _videoService.DidNotReceive().DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when video not found")]
    public async Task ShouldReturnErrorWhenVideoNotFound()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();

        _videoService.DeleteAsync(Arg.Any<Id>())
            .Returns(RequestResult<Video>.WithError("Video not found"));

        // Act
        var result = await _useCase.ExecuteAsync(videoId);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Video not found");
        await _videoService.Received(1).DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when delete operation fails")]
    public async Task ShouldReturnErrorWhenDeleteOperationFails()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();

        _videoService.DeleteAsync(Arg.Any<Id>())
            .Returns(RequestResult<Video>.WithError("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync(videoId);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        await _videoService.Received(1).DeleteAsync(Arg.Any<Id>());
    }

    private static Video CreateValidVideo(string id)
    {
        var videoId = Id.Create(Guid.Parse(id));
        var userId = Id.Create(Guid.NewGuid());
        var video = Video.Create("Test Video", "Test Description", "https://example.com/video.mp4", userId).Data!;
        return video;
    }
}
