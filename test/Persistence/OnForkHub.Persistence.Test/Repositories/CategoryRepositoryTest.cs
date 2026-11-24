using OnForkHub.Persistence.Contexts;
using OnForkHub.Persistence.Repositories;

namespace OnForkHub.Persistence.Test.Repositories;

public class CategoryRepositoryTest : IDisposable
{
    public CategoryRepositoryTest()
    {
        var options = new DbContextOptionsBuilder<EntityFrameworkDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EntityFrameworkDataContext(options);
        _categoryRepository = new CategoryRepositoryEF(_context);
    }

    private readonly CategoryRepositoryEF _categoryRepository;

    private readonly EntityFrameworkDataContext _context;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category successfully")]
    public async Task ShouldCreateCategorySuccessfully()
    {
        var category = Category.Create(Name.Create("Test Category"), "Test Description").Data!;

        var result = await _categoryRepository.CreateAsync(category);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(category);
        _context.Categories.Count().Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should delete category successfully")]
    public async Task ShouldDeleteCategorySuccessfully()
    {
        var category = Category.Create(Name.Create("Test Category"), "Test Description").Data!;
        await _categoryRepository.CreateAsync(category);

        Id id = category.Id;
        var result = await _categoryRepository.DeleteAsync(id);

        result.Status.Should().Be(EResultStatus.Success);
        _context.Categories.Count().Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return category by id successfully")]
    public async Task ShouldReturnCategoryByIdSuccessfully()
    {
        var category = Category.Create(Name.Create("Test Category"), "Test Description").Data!;
        await _categoryRepository.CreateAsync(category);

        Id id = category.Id;
        var result = await _categoryRepository.GetByIdAsync(id);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(category.Id);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when category not found for delete")]
    public async Task ShouldReturnErrorWhenCategoryNotFoundForDelete()
    {
        Id id = Guid.NewGuid().ToString("N");

        var result = await _categoryRepository.DeleteAsync(id);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("not found with ID");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when category not found in GetById")]
    public async Task ShouldReturnErrorWhenCategoryNotFoundInGetById()
    {
        Id id = Guid.NewGuid().ToString("N");

        var result = await _categoryRepository.GetByIdAsync(id);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("not found with ID");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when category is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenCategoryIsNull()
    {
        var act = () => _categoryRepository.CreateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should find created category by its plain GUID id")]
    public async Task ShouldFindCreatedCategoryByPlainGuidId()
    {
        var category = Category.Create(Name.Create("Test Category"), "Test Description").Data!;
        await _categoryRepository.CreateAsync(category);

        category.Id.Should().NotContain("/");
        Guid.TryParse(category.Id, out _).Should().BeTrue("entity ids must be plain GUIDs");

        Id id = category.Id;
        var result = await _categoryRepository.GetByIdAsync(id);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
    }
}
