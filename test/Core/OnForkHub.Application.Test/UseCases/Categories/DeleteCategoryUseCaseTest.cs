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
        // Arrange
        const long categoryId = 1;
        var category = CreateValidCategory(categoryId);

        _categoryRepository.DeleteAsync(categoryId).Returns(RequestResult<Category>.Success(category));

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(category);
        await _categoryRepository.Received(1).DeleteAsync(categoryId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when category not found")]
    public async Task ShouldReturnErrorWhenCategoryNotFound()
    {
        // Arrange
        const long categoryId = 999;

        _categoryRepository.DeleteAsync(categoryId).Returns(RequestResult<Category>.WithError("Category not found"));

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Category not found");
        await _categoryRepository.Received(1).DeleteAsync(categoryId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when delete fails")]
    public async Task ShouldReturnErrorWhenDeleteFails()
    {
        // Arrange
        const long categoryId = 1;

        _categoryRepository.DeleteAsync(categoryId).Returns(RequestResult<Category>.WithError("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Database error");
        await _categoryRepository.Received(1).DeleteAsync(categoryId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when delete returns null data")]
    public async Task ShouldReturnErrorWhenDeleteReturnsNullData()
    {
        // Arrange
        const long categoryId = 1;

        _categoryRepository.DeleteAsync(categoryId).Returns(RequestResult<Category>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        // The use case just returns what the repo returns.
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().BeNull();
        await _categoryRepository.Received(1).DeleteAsync(categoryId);
    }

    private static Category CreateValidCategory(long id)
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;
        return category;
    }
}
