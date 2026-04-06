using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.UseCases.Categories;

namespace OnForkHub.Application.Test.UseCases.Categories;

public class GetAllCategoryUseCaseTest
{
    private readonly ICategoryServiceRavenDB _categoryService;
    private readonly GetAllCategoryUseCase _useCase;

    public GetAllCategoryUseCaseTest()
    {
        _categoryService = Substitute.For<ICategoryServiceRavenDB>();
        _useCase = new GetAllCategoryUseCase(_categoryService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should get all categories successfully")]
    public async Task ShouldGetAllCategoriesSuccessfully()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };
        var categories = CreateValidCategoriesList(3);

        _categoryService.GetAllAsync(request.Page, request.ItemsPerPage)
            .Returns(RequestResult<IEnumerable<Category>>.Success(categories));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        await _categoryService.Received(1).GetAllAsync(request.Page, request.ItemsPerPage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return no content when categories list is null")]
    public async Task ShouldReturnNoContentWhenCategoriesListIsNull()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };

        _categoryService.GetAllAsync(request.Page, request.ItemsPerPage)
            .Returns(RequestResult<IEnumerable<Category>>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.NoContent);
        await _categoryService.Received(1).GetAllAsync(request.Page, request.ItemsPerPage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return no content when service returns null result")]
    public async Task ShouldReturnNoContentWhenServiceReturnsNullResult()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };

        _categoryService.GetAllAsync(request.Page, request.ItemsPerPage)
            .Returns((RequestResult<IEnumerable<Category>>?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.NoContent);
        await _categoryService.Received(1).GetAllAsync(request.Page, request.ItemsPerPage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return empty list when no categories exist")]
    public async Task ShouldReturnEmptyListWhenNoCategoriesExist()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };
        var emptyList = Enumerable.Empty<Category>().ToList();

        _categoryService.GetAllAsync(request.Page, request.ItemsPerPage)
            .Returns(RequestResult<IEnumerable<Category>>.Success(emptyList));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        await _categoryService.Received(1).GetAllAsync(request.Page, request.ItemsPerPage);
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
        await _categoryService.DidNotReceive().GetAllAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should handle pagination correctly")]
    public async Task ShouldHandlePaginationCorrectly()
    {
        // Arrange
        var request = new PaginationRequestDto { Page = 5, ItemsPerPage = 50 };
        var categories = CreateValidCategoriesList(10);

        _categoryService.GetAllAsync(request.Page, request.ItemsPerPage)
            .Returns(RequestResult<IEnumerable<Category>>.Success(categories));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().HaveCount(10);
        await _categoryService.Received(1).GetAllAsync(5, 50);
    }

    private static List<Category> CreateValidCategoriesList(int count)
    {
        var categories = new List<Category>();
        for (var i = 0; i < count; i++)
        {
            var name = Name.Create($"Category {i + 1}");
            var category = Category.Create(name, $"Description {i + 1}").Data!;
            categories.Add(category);
        }

        return categories;
    }
}
