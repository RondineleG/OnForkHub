namespace OnForkHub.Core.Test.Entities;

public class CategoryTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category successfully when data is valid")]
    public void ShouldCreateCategorySuccessfullyWhenDataIsValid()
    {
        var name = Name.Create("Category Test");
        var description = "Category description";

        var result = Category.Create(name, description);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(name);
        result.Data.Description.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should load category successfully when data is valid")]
    public void ShouldLoadCategorySuccessfullyWhenDataIsValid()
    {
        var id = Id.Create();
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.UtcNow;

        var result = Category.Load(id, name, description, createdAt);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(id);
        result.Data.Name.Should().Be(name);
        result.Data.Description.Should().Be(description);
        result.Data.CreatedAt.Should().Be(createdAt);
        result.Data.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should load category with valid ID")]
    public void ShouldLoadCategoryWithValidId()
    {
        var id = Id.Create();
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.UtcNow;

        var result = Category.Load(id, name, description, createdAt);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(id);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not load category with empty ID")]
    public void ShouldNotLoadCategoryWithEmptyId()
    {
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.UtcNow;

        Action action = () => Category.Load(string.Empty, name, description, createdAt);

        action.Should().Throw<DomainException>().WithMessage(IdResources.IdEmpty);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not load category with invalid ID format")]
    public void ShouldNotLoadCategoryWithInvalidIdFormat()
    {
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.UtcNow;

        Action action = () => Category.Load("invalid-guid", name, description, createdAt);

        action.Should().Throw<DomainException>().WithMessage(IdResources.InvalidIdFormat);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should load category with UpdatedAt successfully")]
    public void ShouldLoadCategoryWithUpdatedAtSuccessfully()
    {
        var id = Id.Create();
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.UtcNow;
        var updatedAt = createdAt.AddHours(1);

        var result = Category.Load(id, name, description, createdAt, updatedAt);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(id);
        result.Data.Name.Should().Be(name);
        result.Data.Description.Should().Be(description);
        result.Data.CreatedAt.Should().Be(createdAt);
        result.Data.UpdatedAt.Should().Be(updatedAt);
        result.Data.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        result.Data.UpdatedAt.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate all required properties when loading category")]
    public void ShouldValidateAllRequiredPropertiesWhenLoadingCategory()
    {
        var result = Category.Load(Id.Create(), null!, null!, DateTime.UtcNow);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Name is required").And.Contain("Description is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate id format when loading category")]
    public void ShouldValidateIdFormatWhenLoadingCategory()
    {
        Action action = () => Category.Load("invalid-guid", Name.Create("Test"), "Description", DateTime.UtcNow);

        action.Should().Throw<DomainException>().WithMessage(IdResources.InvalidIdFormat);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update category data successfully")]
    public void ShouldUpdateCategoryDataSuccessfully()
    {
        var originalCategory = Category.Create(Name.Create("Original Category"), "Original description").Data!;

        var newName = Name.Create("Updated Category");
        var newDescription = "Updated description";

        var result = originalCategory.UpdateCategory(newName, newDescription);

        result.Status.Should().Be(EResultStatus.Success);
        originalCategory.Name.Should().Be(newName);
        originalCategory.Description.Should().Be(newDescription);
        originalCategory.UpdatedAt.Should().NotBeNull();
        originalCategory.UpdatedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }
}
