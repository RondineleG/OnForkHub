namespace OnForkHub.Application.Test.UseCases;

using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Application.Dtos.Video.Response;
using OnForkHub.Application.UseCases.Videos;
using OnForkHub.Core.ValueObjects;

public class SearchVideoUseCaseTests
{
    private readonly IVideoRepositoryEF _repository;
    private readonly SearchVideoUseCase _useCase;

    public SearchVideoUseCaseTests()
    {
        _repository = Substitute.For<IVideoRepositoryEF>();
        _useCase = new SearchVideoUseCase(_repository);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should throw ArgumentNullException when request is null")]
    public async Task ExecuteAsyncShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        VideoSearchRequestDto? request = null;

        var act = async () => await _useCase.ExecuteAsync(request!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should return success with paged results")]
    public async Task ExecuteAsyncShouldReturnSuccessWithPagedResults()
    {
        var videos = new List<Video>
        {
            Video.Create("Video 1", "Description 1", "https://example.com/video1.mp4", Id.Create()).Data!,
            Video.Create("Video 2", "Description 2", "https://example.com/video2.mp4", Id.Create()).Data!,
        };

        var request = new VideoSearchRequestDto
        {
            SearchTerm = "Video",
            Page = 1,
            ItemsPerPage = 10,
        };

        _repository
            .SearchAsync(
                request.SearchTerm,
                request.CategoryId,
                request.UserId,
                request.FromDate,
                request.ToDate,
                (int)request.SortBy,
                request.SortDescending,
                request.Page,
                request.ItemsPerPage
            )
            .Returns(RequestResult<(IEnumerable<Video> Items, int TotalCount)>.Success((videos, 2)));

        var result = await _useCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.TotalItems.Should().Be(2);
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should return error when repository fails")]
    public async Task ExecuteAsyncShouldReturnErrorWhenRepositoryFails()
    {
        var request = new VideoSearchRequestDto
        {
            SearchTerm = "Test",
            Page = 1,
            ItemsPerPage = 10,
        };

        _repository
            .SearchAsync(
                Arg.Any<string?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<DateTime?>(),
                Arg.Any<DateTime?>(),
                Arg.Any<int>(),
                Arg.Any<bool>(),
                Arg.Any<int>(),
                Arg.Any<int>()
            )
            .Returns(RequestResult<(IEnumerable<Video> Items, int TotalCount)>.WithError("Database error"));

        var result = await _useCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should handle empty search results")]
    public async Task ExecuteAsyncShouldHandleEmptySearchResults()
    {
        var request = new VideoSearchRequestDto
        {
            SearchTerm = "NonExistent",
            Page = 1,
            ItemsPerPage = 10,
        };

        var emptyVideos = Enumerable.Empty<Video>();

        _repository
            .SearchAsync(
                request.SearchTerm,
                request.CategoryId,
                request.UserId,
                request.FromDate,
                request.ToDate,
                (int)request.SortBy,
                request.SortDescending,
                request.Page,
                request.ItemsPerPage
            )
            .Returns(RequestResult<(IEnumerable<Video> Items, int TotalCount)>.Success((emptyVideos, 0)));

        var result = await _useCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data.TotalItems.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should filter by category ID")]
    public async Task ExecuteAsyncShouldFilterByCategoryId()
    {
        var request = new VideoSearchRequestDto
        {
            CategoryId = 123,
            Page = 1,
            ItemsPerPage = 10,
        };

        var emptyVideos = Enumerable.Empty<Video>();

        _repository
            .SearchAsync(null, 123L, null, null, null, (int)VideoSortField.CreatedAt, true, 1, 10)
            .Returns(RequestResult<(IEnumerable<Video> Items, int TotalCount)>.Success((emptyVideos, 0)));

        await _useCase.ExecuteAsync(request);

        await _repository.Received(1).SearchAsync(null, 123L, null, null, null, (int)VideoSortField.CreatedAt, true, 1, 10);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should filter by user ID")]
    public async Task ExecuteAsyncShouldFilterByUserId()
    {
        var request = new VideoSearchRequestDto
        {
            UserId = "user-123",
            Page = 1,
            ItemsPerPage = 10,
        };

        var emptyVideos = Enumerable.Empty<Video>();

        _repository
            .SearchAsync(null, null, "user-123", null, null, (int)VideoSortField.CreatedAt, true, 1, 10)
            .Returns(RequestResult<(IEnumerable<Video> Items, int TotalCount)>.Success((emptyVideos, 0)));

        await _useCase.ExecuteAsync(request);

        await _repository.Received(1).SearchAsync(null, null, "user-123", null, null, (int)VideoSortField.CreatedAt, true, 1, 10);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should filter by date range")]
    public async Task ExecuteAsyncShouldFilterByDateRange()
    {
        var fromDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var request = new VideoSearchRequestDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            Page = 1,
            ItemsPerPage = 10,
        };

        var emptyVideos = Enumerable.Empty<Video>();

        _repository
            .SearchAsync(null, null, null, fromDate, toDate, (int)VideoSortField.CreatedAt, true, 1, 10)
            .Returns(RequestResult<(IEnumerable<Video> Items, int TotalCount)>.Success((emptyVideos, 0)));

        await _useCase.ExecuteAsync(request);

        await _repository.Received(1).SearchAsync(null, null, null, fromDate, toDate, (int)VideoSortField.CreatedAt, true, 1, 10);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should use default sort values")]
    public async Task ExecuteAsyncShouldUseDefaultSortValues()
    {
        var request = new VideoSearchRequestDto { Page = 1, ItemsPerPage = 10 };

        var emptyVideos = Enumerable.Empty<Video>();

        _repository
            .SearchAsync(null, null, null, null, null, (int)VideoSortField.CreatedAt, true, 1, 10)
            .Returns(RequestResult<(IEnumerable<Video> Items, int TotalCount)>.Success((emptyVideos, 0)));

        await _useCase.ExecuteAsync(request);

        await _repository.Received(1).SearchAsync(null, null, null, null, null, (int)VideoSortField.CreatedAt, true, 1, 10);
    }
}
