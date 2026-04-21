using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Application.UseCases.Videos;

namespace OnForkHub.Application.Test.UseCases.Videos;

public class UpdateVideoUseCaseTest
{
    private readonly IVideoService _videoService;
    private readonly IEntityValidator<Video> _validator;
    private readonly UpdateVideoUseCase _useCase;

    public UpdateVideoUseCaseTest()
    {
        _videoService = Substitute.For<IVideoService>();
        _validator = Substitute.For<IEntityValidator<Video>>();
        _useCase = new UpdateVideoUseCase(_videoService, _validator);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update video successfully")]
    public async Task ShouldUpdateVideoSuccessfully()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        var existingVideo = CreateValidVideo(request.Id);

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(existingVideo));
        _validator.ValidateUpdate(existingVideo).Returns(ValidationResult.Success());
        _videoService.UpdateAsync(existingVideo).Returns(RequestResult<Video>.Success(existingVideo));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        await _videoService.Received(1).GetByIdAsync(Arg.Any<Id>());
        await _videoService.Received(1).UpdateAsync(existingVideo);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when request is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        // Arrange
        VideoUpdateRequestDto? request = null;

        // Act
        var act = () => _useCase.ExecuteAsync(request!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        await _videoService.DidNotReceive().GetByIdAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when videoId has invalid guid format")]
    public async Task ShouldReturnErrorWhenVideoIdHasInvalidGuidFormat()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        request.Id = "invalid-guid";

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Invalid video ID format");
        await _videoService.DidNotReceive().GetByIdAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when video not found")]
    public async Task ShouldReturnErrorWhenVideoNotFound()
    {
        // Arrange
        var request = CreateValidUpdateRequest();

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.WithError("Video not found"));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Video not found");
        await _videoService.Received(1).GetByIdAsync(Arg.Any<Id>());
        await _videoService.DidNotReceive().UpdateAsync(Arg.Any<Video>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when service returns null data on get")]
    public async Task ShouldReturnErrorWhenServiceReturnsNullDataOnGet()
    {
        // Arrange
        var request = CreateValidUpdateRequest();

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        await _videoService.DidNotReceive().UpdateAsync(Arg.Any<Video>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when update video operation fails")]
    public async Task ShouldReturnErrorWhenUpdateVideoOperationFails()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        var existingVideo = CreateValidVideo(request.Id);

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(existingVideo));

        // Force update to fail with empty title
        request.Title = string.Empty;

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return validation errors when validation fails")]
    public async Task ShouldReturnValidationErrorsWhenValidationFails()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        var existingVideo = CreateValidVideo(request.Id);

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(existingVideo));
        _validator.ValidateUpdate(existingVideo).Returns(ValidationResult.Failure("Title is required", "Title"));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasValidation);
        result.Validations.Should().HaveCount(1);
        result.Validations.First().PropertyName.Should().Be("Title");
        result.Validations.First().Description.Should().Be("Title is required");
        _validator.Received(1).ValidateUpdate(existingVideo);
        await _videoService.DidNotReceive().UpdateAsync(Arg.Any<Video>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when save operation fails")]
    public async Task ShouldReturnErrorWhenSaveOperationFails()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        var existingVideo = CreateValidVideo(request.Id);

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(existingVideo));
        _validator.ValidateUpdate(existingVideo).Returns(ValidationResult.Success());
        _videoService.UpdateAsync(existingVideo).Returns(RequestResult<Video>.WithError("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Database error");
        await _videoService.Received(1).UpdateAsync(existingVideo);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when save returns null data")]
    public async Task ShouldReturnErrorWhenSaveReturnsNullData()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        var existingVideo = CreateValidVideo(request.Id);

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(existingVideo));
        _validator.ValidateUpdate(existingVideo).Returns(ValidationResult.Success());
        _videoService.UpdateAsync(existingVideo).Returns(RequestResult<Video>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        await _videoService.Received(1).UpdateAsync(existingVideo);
    }

    private static VideoUpdateRequestDto CreateValidUpdateRequest()
    {
        return new VideoUpdateRequestDto
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Updated Video Title",
            Description = "Updated Description",
            Url = "https://example.com/updated-video.mp4",
            CategoryIds = [],
        };
    }

    private static Video CreateValidVideo(string id)
    {
        var userId = Id.Create();
        var video = Video.Create("Original Video", "Original Description", "https://example.com/original.mp4", userId).Data!;
        return video;
    }
}
