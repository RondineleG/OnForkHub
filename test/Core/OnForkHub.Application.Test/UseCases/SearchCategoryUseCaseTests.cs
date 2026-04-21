namespace OnForkHub.Application.Test.UseCases;

using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.Dtos.Category.Request;
using OnForkHub.Application.Dtos.Category.Response;
using OnForkHub.Application.UseCases.Categories;

public class SearchCategoryUseCaseTests
{
    private readonly ICategoryRepositoryEF _repository;
    private readonly SearchCategoryUseCase _useCase;

    public SearchCategoryUseCaseTests()
    {
        _repository = Substitute.For<ICategoryRepositoryEF>();
        _useCase = new SearchCategoryUseCase(_repository);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should throw ArgumentNullException when request is null")]
    public async Task ExecuteAsyncShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        CategorySearchRequestDto? request = null;

        var act = async () => await _useCase.ExecuteAsync(request!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should return success with paged results")]
    public async Task ExecuteAsyncShouldReturnSuccessWithPagedResults()
    {
        var categories = new List<Category>
        {
            Category.Create(Name.Create("Category 1"), "Description 1").Data!,
            Category.Create(Name.Create("Category 2"), "Description 2").Data!,
        };
        SetCategoryId(categories[0], "Categories/1");
        SetCategoryId(categories[1], "Categories/2");

        var request = new CategorySearchRequestDto
        {
            SearchTerm = "Category",
            Page = 1,
            ItemsPerPage = 10,
        };

        _repository
            .SearchAsync(request.SearchTerm, (int)request.SortBy, request.SortDescending, request.Page, request.ItemsPerPage)
            .Returns(RequestResult<(IEnumerable<Category> Items, int TotalCount)>.Success((categories, 2)));

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
        var request = new CategorySearchRequestDto
        {
            SearchTerm = "Test",
            Page = 1,
            ItemsPerPage = 10,
        };

        _repository
            .SearchAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(RequestResult<(IEnumerable<Category> Items, int TotalCount)>.WithError("Database error"));

        var result = await _useCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should handle empty search results")]
    public async Task ExecuteAsyncShouldHandleEmptySearchResults()
    {
        var request = new CategorySearchRequestDto
        {
            SearchTerm = "NonExistent",
            Page = 1,
            ItemsPerPage = 10,
        };

        var emptyCategories = Enumerable.Empty<Category>();

        _repository
            .SearchAsync(request.SearchTerm, (int)request.SortBy, request.SortDescending, request.Page, request.ItemsPerPage)
            .Returns(RequestResult<(IEnumerable<Category> Items, int TotalCount)>.Success((emptyCategories, 0)));

        var result = await _useCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data.TotalItems.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ExecuteAsync should use default sort values")]
    public async Task ExecuteAsyncShouldUseDefaultSortValues()
    {
        var request = new CategorySearchRequestDto { Page = 1, ItemsPerPage = 10 };

        var categories = new List<Category>();

        _repository
            .SearchAsync(null, (int)CategorySortField.Name, false, 1, 10)
            .Returns(RequestResult<(IEnumerable<Category> Items, int TotalCount)>.Success((categories, 0)));

        await _useCase.ExecuteAsync(request);

        await _repository.Received(1).SearchAsync(null, (int)CategorySortField.Name, false, 1, 10);
    }

    private static void SetCategoryId(Category category, string id)
    {
        var idProperty = typeof(Category).GetProperty("Id");
        idProperty?.SetValue(category, id);
    }
}
