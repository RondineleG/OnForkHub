namespace OnForkHub.Persistence.Test.Repositories;

public class CategoryRepositoryTest
{
    private readonly CategoryRepositoryEF _categoryRepository;

    private readonly IEntityFrameworkDataContext _context;

    public CategoryRepositoryTest()
    {
        _context = Substitute.For<IEntityFrameworkDataContext>();
        _categoryRepository = new CategoryRepositoryEF(_context);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category successfully")]
    public async Task ShouldCreateCategorySuccessfully()
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;
        var dbSetMock = Substitute.For<DbSet<Category>>();
        _context.Categories.Returns(dbSetMock);

        var result = await _categoryRepository.CreateAsync(category);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(category);
        await _context.Received(1).SaveChangesAsync();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should delete category successfully")]
    public async Task ShouldDeleteCategorySuccessfully()
    {
        long categoryId = 1;
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;
        var dbSetMock = Substitute.For<DbSet<Category>>();
        dbSetMock.FindAsync(categoryId).Returns(category);
        _context.Categories.Returns(dbSetMock);

        var result = await _categoryRepository.DeleteAsync(categoryId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(category);
        await _context.Received(1).SaveChangesAsync();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return category by id successfully")]
    public async Task ShouldReturnCategoryByIdSuccessfully()
    {
        long categoryId = 1;
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;
        var dbSetMock = Substitute.For<DbSet<Category>>();
        dbSetMock.FindAsync(categoryId).Returns(category);
        _context.Categories.Returns(dbSetMock);

        var result = await _categoryRepository.GetByIdAsync(categoryId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(category);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when category not found for delete")]
    public async Task ShouldReturnErrorWhenCategoryNotFoundForDelete()
    {
        long categoryId = 1;
        var dbSetMock = Substitute.For<DbSet<Category>>();
        dbSetMock.FindAsync(categoryId).Returns((Category?)null);
        _context.Categories.Returns(dbSetMock);

        var result = await _categoryRepository.DeleteAsync(categoryId);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("not found with ID");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when category not found in GetById")]
    public async Task ShouldReturnErrorWhenCategoryNotFoundInGetById()
    {
        long categoryId = 1;
        var dbSetMock = Substitute.For<DbSet<Category>>();
        dbSetMock.FindAsync(categoryId).Returns((Category?)null);
        _context.Categories.Returns(dbSetMock);

        var result = await _categoryRepository.GetByIdAsync(categoryId);

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
    [DisplayName("Should throw exception when creating duplicate category")]
    public async Task ShouldThrowExceptionWhenCreatingDuplicateCategory()
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;
        var dbSetMock = Substitute.For<DbSet<Category>>();
        _context.Categories.Returns(dbSetMock);

        var innerException = new Exception("duplicate key");
        _context.SaveChangesAsync().ThrowsAsync(new DbUpdateException("Error", innerException));

        var act = () => _categoryRepository.CreateAsync(category);

        await act.Should().ThrowAsync<UniqueConstraintException>().WithMessage("*with the same unique data already exists*");
    }
}
