namespace OnForkHub.Application.Test.Dtos;

using OnForkHub.Application.Dtos.Category.Response;

public class CategorySearchResponseTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("FromCategory should map all properties correctly")]
    public void FromCategoryShouldMapAllPropertiesCorrectly()
    {
        var category = Category.Create(Name.Create("Test Category"), "Test Description").Data!;

        var dto = CategorySearchResponse.FromCategory(category);

        dto.Id.Should().Be(category.Id);
        Guid.TryParse(dto.Id, out _).Should().BeTrue("category ids are plain GUIDs");
        dto.Name.Should().Be("Test Category");
        dto.Description.Should().Be("Test Description");
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Default constructor should initialize properties to defaults")]
    public void DefaultConstructorShouldInitializePropertiesToDefaults()
    {
        var dto = new CategorySearchResponse();

        dto.Id.Should().Be(string.Empty);
        dto.Name.Should().Be(string.Empty);
        dto.Description.Should().Be(string.Empty);
        dto.UpdatedAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set all properties correctly")]
    public void ShouldSetAllPropertiesCorrectly()
    {
        var id = Guid.NewGuid().ToString("N");
        var dto = new CategorySearchResponse
        {
            Id = id,
            Name = "Test Category",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Test Category");
        dto.Description.Should().Be("Test Description");
        dto.UpdatedAt.Should().NotBeNull();
    }
}
