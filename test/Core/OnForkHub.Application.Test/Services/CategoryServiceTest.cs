namespace OnForkHub.Application.Test.Services;

public class CategoryServiceTest
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidationService _validationService;
    private readonly CategoryService _categoryService;

    public CategoryServiceTest()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _validationService = Substitute.For<IValidationService>();
        _categoryService = new CategoryService(_categoryRepository, _validationService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category successfully")]
    public async Task ShouldCreateCategorySuccessfully()
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        _categoryRepository.CreateAsync(category).Returns(RequestResult<Category>.Success(category));

        var result = await _categoryService.CreateAsync(category);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(category);
        await _categoryRepository.Received(1).CreateAsync(category);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update category successfully")]
    public async Task ShouldUpdateCategorySuccessfully()
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        _categoryRepository.UpdateAsync(category).Returns(RequestResult<Category>.Success(category));

        var result = await _categoryService.UpdateAsync(category);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(category);
        await _categoryRepository.Received(1).UpdateAsync(category);
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
        int page = 1;
        int size = 10;
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
}
