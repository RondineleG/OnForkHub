using OnForkHub.Application.UseCases.Categories;

namespace OnForkHub.Application.Test.UseCases.Categories;

public class GetByIdCategoryUseCaseTest
{
    private readonly ICategoryRepositoryEF _categoryRepository;
    private readonly GetByIdCategoryUseCase _useCase;

    public GetByIdCategoryUseCaseTest()
    {
        _categoryRepository = Substitute.For<ICategoryRepositoryEF>();
        _useCase = new GetByIdCategoryUseCase(_categoryRepository);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return category when found")]
    public async Task ShouldReturnCategoryWhenFound()
    {
        var categoryId = Guid.NewGuid().ToString("N");
        var category = CreateValidCategory();

        _categoryRepository.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Category>.Success(category));

        var result = await _useCase.ExecuteAsync(categoryId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when id is not a valid guid")]
    public async Task ShouldReturnErrorWhenIdIsNotValidGuid()
    {
        var result = await _useCase.ExecuteAsync("not-a-guid");

        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Invalid category ID format");
        await _categoryRepository.DidNotReceive().GetByIdAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return no content when category not found")]
    public async Task ShouldReturnNoContentWhenCategoryNotFound()
    {
        var categoryId = Guid.NewGuid().ToString("N");

        _categoryRepository.GetByIdAsync(Arg.Any<Id>()).Returns(RequestResult<Category>.WithError("Category not found"));

        var result = await _useCase.ExecuteAsync(categoryId);

        result.Status.Should().Be(EResultStatus.NoContent);
    }

    private static Category CreateValidCategory()
    {
        var name = Name.Create("Test Category");
        return Category.Create(name, "Test Description").Data!;
    }
}
