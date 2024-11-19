namespace OnForkHub.Application.Test.Services;

public class CategoryServiceTest
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidationService<Category> _validationService;
    private readonly CategoryService _categoryService;

    public CategoryServiceTest()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _validationService = Substitute.For<IValidationService<Category>>();
        _categoryService = new CategoryService(_categoryRepository, _validationService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not create category when validation fails")]
    public async Task ShouldNotCreateCategoryWhenValidationFails()
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        var validationResult = new ValidationResult();
        validationResult.AddError("Test error", "TestField");

        _validationService.Validate(Arg.Any<Category>()).Returns(validationResult);

        var result = await _categoryService.CreateAsync(category);

        result.Status.Should().Be(EResultStatus.HasValidation);
        result.Validations.Should().Contain(v => v.Description == "Test error" && v.PropertyName == "TestField");

        await _categoryRepository.DidNotReceive().CreateAsync(Arg.Any<Category>());
        _validationService.Received(1).Validate(Arg.Is<Category>(c => c == category));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not update category when validation fails")]
    public async Task ShouldNotUpdateCategoryWhenValidationFails()
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        var validationResult = new ValidationResult();
        validationResult.AddError("Test error", "TestField");

        _validationService.ValidateUpdate(Arg.Any<Category>()).Returns(validationResult);

        var result = await _categoryService.UpdateAsync(category);

        result.Status.Should().Be(EResultStatus.HasValidation);
        result.Validations.Should().Contain(v => v.Description == "Test error" && v.PropertyName == "TestField");

        await _categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
        _validationService.Received(1).ValidateUpdate(Arg.Is<Category>(c => c == category));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update category when validation passes")]
    public async Task ShouldUpdateCategoryWhenValidationPasses()
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        _validationService.ValidateUpdate(Arg.Any<Category>()).Returns(new ValidationResult());

        _categoryRepository.UpdateAsync(category).Returns(RequestResult<Category>.Success(category));

        var result = await _categoryService.UpdateAsync(category);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(category);

        await _categoryRepository.Received(1).UpdateAsync(Arg.Is<Category>(c => c == category));
        _validationService.Received(1).ValidateUpdate(Arg.Is<Category>(c => c == category));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when category not found for delete")]
    public async Task ShouldReturnErrorWhenCategoryNotFoundForDelete()
    {
        long categoryId = 1;

        _categoryRepository.GetByIdAsync(categoryId).Returns(RequestResult<Category>.WithError(new RequestError("Category not found")));

        var result = await _categoryService.DeleteAsync(categoryId);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Category not found");
        await _categoryRepository.Received(1).GetByIdAsync(categoryId);
        await _categoryRepository.DidNotReceive().DeleteAsync(categoryId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should delete category successfully")]
    public async Task ShouldDeleteCategorySuccessfully()
    {
        long categoryId = 1;
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        _categoryRepository.GetByIdAsync(categoryId).Returns(RequestResult<Category>.Success(category));
        _categoryRepository.DeleteAsync(categoryId).Returns(RequestResult<Category>.Success(category));

        var result = await _categoryService.DeleteAsync(categoryId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(category);
        await _categoryRepository.Received(1).GetByIdAsync(categoryId);
        await _categoryRepository.Received(1).DeleteAsync(categoryId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return category by id successfully")]
    public async Task ShouldReturnCategoryByIdSuccessfully()
    {
        long categoryId = 1;
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        _categoryRepository.GetByIdAsync(categoryId).Returns(RequestResult<Category>.Success(category));

        var result = await _categoryService.GetByIdAsync(categoryId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(category);
        await _categoryRepository.Received(1).GetByIdAsync(categoryId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return categories successfully")]
    public async Task ShouldReturnCategoriesSuccessfully()
    {
        var page = 1;
        var size = 10;
        var categories = new List<Category>
        {
            Category.Create(Name.Create("Category 1"), "Description 1").Data!,
            Category.Create(Name.Create("Category 2"), "Description 2").Data!,
        };

        _categoryRepository.GetAsync(page, size).Returns(RequestResult<IEnumerable<Category>>.Success(categories));

        var result = await _categoryService.GetAsync(page, size);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().BeEquivalentTo(categories);
        await _categoryRepository.Received(1).GetAsync(page, size);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should handle null category in update")]
    public async Task ShouldHandleNullCategoryInUpdate()
    {
        Category? category = null;

        var result = await _categoryService.UpdateAsync(category!);

        result.Status.Should().Be(EResultStatus.HasValidation);
        result.Validations.Should().NotBeEmpty();
        result.Validations.Should().Contain(v => v.Description == "Category cannot be null" && v.PropertyName == "Category");

        await _categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>());

        _validationService.DidNotReceive().ValidateUpdate(Arg.Any<Category>());
    }
}
