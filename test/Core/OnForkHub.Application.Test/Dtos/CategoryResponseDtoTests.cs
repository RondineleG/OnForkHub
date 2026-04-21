namespace OnForkHub.Application.Test.Dtos;

using OnForkHub.Application.Dtos.Category.Response;

public class CategoryResponseDtoTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromCategory should throw ArgumentNullException when category is null")]
    public void FromCategoryShouldThrowArgumentNullExceptionWhenCategoryIsNull()
    {
        Category? category = null;

        var act = () => CategoryResponseDto.FromCategory(category!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromCategory should map all properties correctly")]
    public void FromCategoryShouldMapAllPropertiesCorrectly()
    {
        var category = Category.Create(Name.Create("Test Category"), "Test Description").Data!;
        SetCategoryId(category, "Categories/123");

        var dto = CategoryResponseDto.FromCategory(category);

        dto.Id.Should().Be(123);
        dto.Name.Should().Be("Test Category");
        dto.Description.Should().Be("Test Description");
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromCategory should handle ID without collection prefix")]
    public void FromCategoryShouldHandleIdWithoutCollectionPrefix()
    {
        var category = Category.Create(Name.Create("Test Category"), "Test Description").Data!;
        SetCategoryId(category, "456");

        var dto = CategoryResponseDto.FromCategory(category);

        dto.Id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromCategory should handle invalid numeric ID")]
    public void FromCategoryShouldHandleInvalidNumericId()
    {
        var category = Category.Create(Name.Create("Test Category"), "Test Description").Data!;
        SetCategoryId(category, "Categories/invalid");

        var dto = CategoryResponseDto.FromCategory(category);

        dto.Id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Default constructor should initialize properties to defaults")]
    public void DefaultConstructorShouldInitializePropertiesToDefaults()
    {
        var dto = new CategoryResponseDto();

        dto.Id.Should().Be(0);
        dto.Name.Should().Be(string.Empty);
        dto.Description.Should().Be(string.Empty);
        dto.UpdatedAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set all properties correctly")]
    public void ShouldSetAllPropertiesCorrectly()
    {
        var dto = new CategoryResponseDto
        {
            Id = 123,
            Name = "Test Category",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        dto.Id.Should().Be(123);
        dto.Name.Should().Be("Test Category");
        dto.Description.Should().Be("Test Description");
        dto.UpdatedAt.Should().NotBeNull();
    }

    private static void SetCategoryId(Category category, string id)
    {
        var idProperty = typeof(Category).GetProperty("Id");
        idProperty?.SetValue(category, id);
    }
}
