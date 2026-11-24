using OnForkHub.Application.UseCases.Categories;

namespace OnForkHub.Application.Test.UseCases.Categories;

public class DeleteCategoryUseCaseTest
{
    private readonly ICategoryRepositoryEF _categoryRepository;
    private readonly DeleteCategoryUseCase _useCase;

    public DeleteCategoryUseCaseTest()
    {
        _categoryRepository = Substitute.For<ICategoryRepositoryEF>();
        _useCase = new DeleteCategoryUseCase(_categoryRepository);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should delete category successfully")]
    public async Task ShouldDeleteCategorySuccessfully()
    {
        var categoryId = Guid.NewGuid().ToString("N");
        var category = CreateValidCategory();

        _categoryRepository.DeleteAsync(Arg.Any<Id>()).Returns(RequestResult<Category>.Success(category));

        var result = await _useCase.ExecuteAsync(categoryId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        await _categoryRepository.Received(1).DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when id is not a valid guid")]
    public async Task ShouldReturnErrorWhenIdIsNotValidGuid()
    {
        var result = await _useCase.ExecuteAsync("not-a-guid");

        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Invalid category ID format");
        await _categoryRepository.DidNotReceive().DeleteAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when category not found")]
    public async Task ShouldReturnErrorWhenCategoryNotFound()
    {
        var categoryId = Guid.NewGuid().ToString("N");

        _categoryRepository.DeleteAsync(Arg.Any<Id>()).Returns(RequestResult<Category>.WithError("Category not found"));

        var result = await _useCase.ExecuteAsync(categoryId);

        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Category not found");
    }

    private static Category CreateValidCategory()
    {
        var name = Name.Create("Test Category");
        return Category.Create(name, "Test Description").Data!;
    }
}
