namespace OnForkHub.Application.Test.UseCases;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Application.UseCases.Videos;
using OnForkHub.Core.Interfaces.Services;

public class CreateVideoUseCaseTests
{
    private readonly IVideoService _videoService;
    private readonly IEntityValidator<Video> _validator;
    private readonly CreateVideoUseCase _useCase;

    public CreateVideoUseCaseTests()
    {
        _videoService = Substitute.For<IVideoService>();
        _validator = Substitute.For<IEntityValidator<Video>>();
        _useCase = new CreateVideoUseCase(_videoService, _validator);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should throw ArgumentNullException when request is null")]
    public async Task ExecuteAsyncShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        VideoCreateRequestDto? request = null;

        var act = async () => await _useCase.ExecuteAsync(request!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should return error when video creation fails")]
    public async Task ExecuteAsyncShouldReturnErrorWhenVideoCreationFails()
    {
        var request = new VideoCreateRequestDto
        {
            Title = string.Empty,
            Description = "Description",
            Url = "https://example.com/video.mp4",
            UserId = "user-123",
        };

        var result = await _useCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should return validation errors when validation fails")]
    public async Task ExecuteAsyncShouldReturnValidationErrorsWhenValidationFails()
    {
        var userId = Id.Create();
        var request = new VideoCreateRequestDto
        {
            Title = "Valid Title",
            Description = "Valid Description",
            Url = "https://example.com/video.mp4",
            UserId = userId.ToString(),
        };

        var validationResult = new ValidationResult();
        validationResult.AddError("Title is too short", "Title");

        _validator.Validate(Arg.Any<Video>()).Returns(validationResult);

        var result = await _useCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasValidation);
        result.Validations.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should return success when video is created")]
    public async Task ExecuteAsyncShouldReturnSuccessWhenVideoIsCreated()
    {
        var userId = Id.Create();
        var request = new VideoCreateRequestDto
        {
            Title = "Valid Video Title",
            Description = "Valid Description",
            Url = "https://example.com/video.mp4",
            UserId = userId.ToString(),
        };

        var validationResult = new ValidationResult();
        _validator.Validate(Arg.Any<Video>()).Returns(validationResult);

        var video = Video.Create("Valid Video Title", "Valid Description", "https://example.com/video.mp4", userId).Data!;

        _videoService.CreateAsync(Arg.Any<Video>()).Returns(RequestResult<Video>.Success(video));

        var result = await _useCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should return error when service fails")]
    public async Task ExecuteAsyncShouldReturnErrorWhenServiceFails()
    {
        var userId = Id.Create();
        var request = new VideoCreateRequestDto
        {
            Title = "Valid Video Title",
            Description = "Valid Description",
            Url = "https://example.com/video.mp4",
            UserId = userId.ToString(),
        };

        var validationResult = new ValidationResult();
        _validator.Validate(Arg.Any<Video>()).Returns(validationResult);

        _videoService.CreateAsync(Arg.Any<Video>()).Returns(RequestResult<Video>.WithError("Service error"));

        var result = await _useCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
    }
}
