using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.UseCases.Videos;

namespace OnForkHub.Application.Test.UseCases.Videos;

public class GetAllVideoUseCaseTest
{
    private readonly IVideoService _videoService;
    private readonly GetAllVideoUseCase _useCase;

    public GetAllVideoUseCaseTest()
    {
        _videoService = Substitute.For<IVideoService>();
        _useCase = new GetAllVideoUseCase(_videoService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should get all videos successfully")]
    public async Task ShouldGetAllVideosSuccessfully()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };
        var videos = CreateValidVideosList(3);

        _videoService
            .GetAllAsync(request.Page, request.ItemsPerPage)
            .Returns((RequestResult<IEnumerable<Video>>?)RequestResult<IEnumerable<Video>>.Success(videos));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        await _videoService.Received(1).GetAllAsync(request.Page, request.ItemsPerPage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return no content when videos list is null")]
    public async Task ShouldReturnNoContentWhenVideosListIsNull()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };

        _videoService.GetAllAsync(request.Page, request.ItemsPerPage).Returns(RequestResult<IEnumerable<Video>>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.NoContent);
        await _videoService.Received(1).GetAllAsync(request.Page, request.ItemsPerPage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return no content when service returns null result")]
    public async Task ShouldReturnNoContentWhenServiceReturnsNullResult()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };

        _videoService.GetAllAsync(request.Page, request.ItemsPerPage).Returns((RequestResult<IEnumerable<Video>>?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.NoContent);
        await _videoService.Received(1).GetAllAsync(request.Page, request.ItemsPerPage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return empty list when no videos exist")]
    public async Task ShouldReturnEmptyListWhenNoVideosExist()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };
        var emptyList = Enumerable.Empty<Video>().ToList();

        _videoService.GetAllAsync(request.Page, request.ItemsPerPage).Returns(RequestResult<IEnumerable<Video>>.Success(emptyList));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        await _videoService.Received(1).GetAllAsync(request.Page, request.ItemsPerPage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when request is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        // Arrange
        PaginationRequestDto? request = null;

        // Act
        var act = () => _useCase.ExecuteAsync(request!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        await _videoService.DidNotReceive().GetAllAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should handle pagination correctly")]
    public async Task ShouldHandlePaginationCorrectly()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 5, ItemsPerPage = 50 };
        var videos = CreateValidVideosList(10);

        _videoService.GetAllAsync(request.Page, request.ItemsPerPage).Returns(RequestResult<IEnumerable<Video>>.Success(videos));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().HaveCount(10);
        await _videoService.Received(1).GetAllAsync(5, 50);
    }

    private static List<Video> CreateValidVideosList(int count)
    {
        var videos = new List<Video>();
        var userId = Id.Create();
        for (var i = 0; i < count; i++)
        {
            var video = Video.Create($"Video {i + 1}", $"Description {i + 1}", $"https://example.com/video{i + 1}.mp4", userId).Data!;
            videos.Add(video);
        }

        return videos;
    }
}
