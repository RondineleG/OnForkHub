using OnForkHub.Application.Dtos.Category.Request;
using OnForkHub.Application.UseCases.Categories;

namespace OnForkHub.Application.Test.UseCases.Categories;

public class UpdateCategoryUseCaseTest
{
    private readonly ICategoryRepositoryEF _categoryRepository;
    private readonly IEntityValidator<Category> _validator;
    private readonly UpdateCategoryUseCase _useCase;

    public UpdateCategoryUseCaseTest()
    {
        _categoryRepository = Substitute.For<ICategoryRepositoryEF>();
        _validator = Substitute.For<IEntityValidator<Category>>();
        _useCase = new UpdateCategoryUseCase(_categoryRepository, _validator);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update category successfully")]
    public async Task ShouldUpdateCategorySuccessfully()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1);
        var existingCategory = CreateValidCategory(1);

        _categoryRepository.GetByIdAsync(request.Id).Returns(RequestResult<Category>.Success(existingCategory));
        _validator.ValidateUpdate(existingCategory).Returns(ValidationResult.Success());
        _categoryRepository.UpdateAsync(existingCategory).Returns(RequestResult<Category>.Success(existingCategory));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        await _categoryRepository.Received(1).GetByIdAsync(request.Id);
        await _categoryRepository.Received(1).UpdateAsync(existingCategory);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when request is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        // Arrange
        CategoryUpdateRequestDto? request = null;

        // Act
        var act = () => _useCase.ExecuteAsync(request!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        await _categoryRepository.DidNotReceive().GetByIdAsync(Arg.Any<long>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when category not found")]
    public async Task ShouldReturnErrorWhenCategoryNotFound()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1);

        _categoryRepository.GetByIdAsync(request.Id).Returns(RequestResult<Category>.WithError("Category not found"));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Category not found");
        await _categoryRepository.Received(1).GetByIdAsync(request.Id);
        await _categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when repository returns null data")]
    public async Task ShouldReturnErrorWhenRepositoryReturnsNullData()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1);

        _categoryRepository.GetByIdAsync(request.Id).Returns(RequestResult<Category>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Category not found");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when update operation fails")]
    public async Task ShouldReturnErrorWhenUpdateOperationFails()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1);
        var existingCategory = CreateValidCategory(1);

        _categoryRepository.GetByIdAsync(request.Id).Returns(RequestResult<Category>.Success(existingCategory));
        _validator.ValidateUpdate(existingCategory).Returns(ValidationResult.Success());
        _categoryRepository.UpdateAsync(existingCategory).Returns(RequestResult<Category>.WithError("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return validation errors when validation fails")]
    public async Task ShouldReturnValidationErrorsWhenValidationFails()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1);
        var existingCategory = CreateValidCategory(1);

        _categoryRepository.GetByIdAsync(request.Id).Returns(RequestResult<Category>.Success(existingCategory));
        _validator.ValidateUpdate(existingCategory).Returns(ValidationResult.Failure("Name is required", "Name"));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasValidation);
        result.Validations.Should().HaveCount(1);
        result.Validations.First().PropertyName.Should().Be("Name");
        result.Validations.First().Description.Should().Be("Name is required");
        _validator.Received(1).ValidateUpdate(existingCategory);
        await _categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when update repository fails")]
    public async Task ShouldReturnErrorWhenUpdateRepositoryFails()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1);
        var existingCategory = CreateValidCategory(1);

        _categoryRepository.GetByIdAsync(request.Id).Returns(RequestResult<Category>.Success(existingCategory));
        _validator.ValidateUpdate(existingCategory).Returns(ValidationResult.Success());
        _categoryRepository.UpdateAsync(existingCategory).Returns(RequestResult<Category>.WithError("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Failed to update category");
        await _categoryRepository.Received(1).UpdateAsync(existingCategory);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when update repository returns null data")]
    public async Task ShouldReturnErrorWhenUpdateRepositoryReturnsNullData()
    {
        // Arrange
        var request = CreateValidUpdateRequest(1);
        var existingCategory = CreateValidCategory(1);

        _categoryRepository.GetByIdAsync(request.Id).Returns(RequestResult<Category>.Success(existingCategory));
        _validator.ValidateUpdate(existingCategory).Returns(ValidationResult.Success());
        _categoryRepository.UpdateAsync(existingCategory).Returns(RequestResult<Category>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Failed to update category");
    }

    private static CategoryUpdateRequestDto CreateValidUpdateRequest(long id)
    {
        return new CategoryUpdateRequestDto
        {
            Id = id,
            Category = new CategoryRequestDto { Name = "Updated Category", Description = "Updated Description" },
        };
    }

    private static Category CreateValidCategory(long id)
    {
        var name = Name.Create("Original Category");
        var category = Category.Create(name, "Original Description").Data!;
        return category;
    }
}
