using OnForkHub.Application.Dtos.Video.Request;
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
    [DisplayName("Should delete video successfully when requester is the owner")]
    public async Task ShouldDeleteVideoSuccessfullyWhenRequesterIsOwner()
    {
        var ownerId = Guid.NewGuid();
        var video = CreateValidVideo(ownerId);
        var videoId = video.Id;

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(video));
        _videoService.DeleteAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(video));

        var result = await _useCase.ExecuteAsync(new VideoDeleteRequest { Id = videoId, RequesterUserId = ownerId.ToString("N") });

        result.Status.Should().Be(EResultStatus.Success);
        await _videoService.Received(1).DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should delete video successfully when requester is admin")]
    public async Task ShouldDeleteVideoSuccessfullyWhenRequesterIsAdmin()
    {
        var ownerId = Guid.NewGuid();
        var video = CreateValidVideo(ownerId);

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(video));
        _videoService.DeleteAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(video));

        var result = await _useCase.ExecuteAsync(
            new VideoDeleteRequest { Id = video.Id, RequesterUserId = Guid.NewGuid().ToString("N"), RequesterIsAdmin = true }
        );

        result.Status.Should().Be(EResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should forbid delete when requester is not the owner")]
    public async Task ShouldForbidDeleteWhenRequesterIsNotOwner()
    {
        var ownerId = Guid.NewGuid();
        var video = CreateValidVideo(ownerId);

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(video));

        var result = await _useCase.ExecuteAsync(new VideoDeleteRequest { Id = video.Id, RequesterUserId = Guid.NewGuid().ToString("N") });

        result.Status.Should().Be(EResultStatus.Forbidden);
        await _videoService.DidNotReceive().DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should forbid delete when requester is unknown")]
    public async Task ShouldForbidDeleteWhenRequesterIsUnknown()
    {
        var ownerId = Guid.NewGuid();
        var video = CreateValidVideo(ownerId);

        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.Success(video));

        var result = await _useCase.ExecuteAsync(new VideoDeleteRequest { Id = video.Id });

        result.Status.Should().Be(EResultStatus.Forbidden);
        await _videoService.DidNotReceive().DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when video id is invalid")]
    public async Task ShouldReturnErrorWhenVideoIdIsInvalid()
    {
        var result = await _useCase.ExecuteAsync(new VideoDeleteRequest { Id = "not-a-guid" });

        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Invalid video ID format");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when video is not found")]
    public async Task ShouldReturnErrorWhenVideoIsNotFound()
    {
        _videoService.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Video>.WithError("not found"));

        var result = await _useCase.ExecuteAsync(new VideoDeleteRequest { Id = Guid.NewGuid().ToString("N"), RequesterUserId = Guid.NewGuid().ToString("N") });

        result.Status.Should().Be(EResultStatus.HasError);
        await _videoService.DidNotReceive().DeleteAsync(Arg.Any<Id>());
    }

    private static Video CreateValidVideo(Guid ownerId)
    {
        Id userId = ownerId.ToString("N");
        return Video.Create("Test video title", "Test video description", "https://example.com/video.mp4", userId).Data!;
    }
}
