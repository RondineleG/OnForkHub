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
    [DisplayName("Should get category by id successfully")]
    public async Task ShouldGetCategoryByIdSuccessfully()
    {
        // Arrange
        const long categoryId = 1;
        var category = CreateValidCategory(categoryId);

        _categoryRepository.GetByIdAsync(categoryId).Returns((RequestResult<Category>?)RequestResult<Category>.Success(category));

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(category);
        await _categoryRepository.Received(1).GetByIdAsync(categoryId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return no content when category not found")]
    public async Task ShouldReturnNoContentWhenCategoryNotFound()
    {
        // Arrange
        const long categoryId = 999;

        _categoryRepository.GetByIdAsync(categoryId).Returns(RequestResult<Category>.WithNoContent());

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        result.Status.Should().Be(EResultStatus.NoContent);
        await _categoryRepository.Received(1).GetByIdAsync(categoryId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return no content when repository returns null result")]
    public async Task ShouldReturnNoContentWhenRepositoryReturnsNullResult()
    {
        // Arrange
        const long categoryId = 1;

        _categoryRepository.GetByIdAsync(categoryId).Returns((RequestResult<Category>?)null);

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        result.Status.Should().Be(EResultStatus.NoContent);
        await _categoryRepository.Received(1).GetByIdAsync(categoryId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return no content when repository returns null data")]
    public async Task ShouldReturnNoContentWhenRepositoryReturnsNullData()
    {
        // Arrange
        const long categoryId = 1;

        _categoryRepository.GetByIdAsync(categoryId).Returns(RequestResult<Category>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync(categoryId);

        // Assert
        result.Status.Should().Be(EResultStatus.NoContent);
        await _categoryRepository.Received(1).GetByIdAsync(categoryId);
    }

    private static Category CreateValidCategory(long id)
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;
        return category;
    }
}
