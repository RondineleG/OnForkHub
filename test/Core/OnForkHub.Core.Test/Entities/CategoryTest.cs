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
        var id = 1L;
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
    [DisplayName("Should throw exception when creating category with empty name")]
    public void ShouldThrowExceptionWhenCreatingCategoryWithEmptyName()
    {
        Action action = () => Name.Create(string.Empty);

        action.Should().Throw<DomainException>().WithMessage(NameResources.NameEmpty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when creating category with invalid name")]
    public void ShouldThrowExceptionWhenCreatingCategoryWithInvalidName(string? invalidName)
    {
        Action action = () => Name.Create(invalidName!);

        action.Should().Throw<DomainException>().WithMessage(NameResources.NameEmpty);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate all required properties when creating category")]
    public void ShouldValidateAllRequiredPropertiesWhenCreatingCategory()
    {
        Name? name = null;
        string? description = null;

        var result = Category.Create(name!, description!);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Name is required").And.Contain("Description is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate name is required")]
    public void ShouldValidateNameIsRequired()
    {
        Name? name = null;
        var description = "Valid Description";

        var result = Category.Create(name!, description);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Name is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate description is required")]
    public void ShouldValidateDescriptionIsRequired()
    {
        var name = Name.Create("Valid Name");
        string? description = null;

        var result = Category.Create(name, description!);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Description is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category with minimum valid data")]
    public void ShouldCreateCategoryWithMinimumValidData()
    {
        var name = Name.Create("Test Category");
        var description = "Test";

        var result = Category.Create(name, description);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(name);
        result.Data.Description.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category with valid data")]
    public void ShouldCreateCategoryWithValidData()
    {
        var name = Name.Create("Test Category");
        var description = "Test Description";

        var result = Category.Create(name, description);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(name);
        result.Data.Description.Should().Be(description);
        result.Message.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [Trait("Category", "Unit")]
    [DisplayName("Should not create category with invalid description")]
    public void ShouldNotCreateCategoryWithInvalidDescription(string? invalidDescription)
    {
        var name = Name.Create("Test Category");

        var result = Category.Create(name, invalidDescription!);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Description is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not create category with too long description")]
    public void ShouldNotCreateCategoryWithTooLongDescription()
    {
        var name = Name.Create("Test Category");
        var description = new string('A', 201);

        var result = Category.Create(name, description);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Description cannot exceed 200 characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category with maximum length description")]
    public void ShouldCreateCategoryWithMaximumLengthDescription()
    {
        var name = Name.Create("Test Category");
        var description = new string('A', 200);

        var result = Category.Create(name, description);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(name);
        result.Data.Description.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should handle null description when creating category")]
    public void ShouldHandleNullDescriptionWhenCreatingCategory()
    {
        var name = Name.Create("Test Category");
        string? description = null;

        var result = Category.Create(name, description!);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Description is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category with valid name")]
    public void ShouldCreateCategoryWithValidName()
    {
        var validName = "Valid Category Name";
        var description = "Category description";

        var result = Category.Create(Name.Create(validName), description);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Value.Should().Be(validName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(long.MaxValue)]
    [Trait("Category", "Unit")]
    [DisplayName("Should load category with valid positive ID")]
    public void ShouldLoadCategoryWithValidPositiveId(long validId)
    {
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.UtcNow;

        var result = Category.Load(validId, name, description, createdAt);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(validId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not load category with minimum negative ID")]
    public void ShouldNotLoadCategoryWithMinimumNegativeId()
    {
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.UtcNow;

        var result = Category.Load(long.MinValue, name, description, createdAt);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Id cannot be negative");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when validating name with more than 50 characters")]
    public void ShouldReturnErrorWhenValidatingNameWithMoreThan50Characters()
    {
        var name = Name.Create(new string('A', 51));
        var description = "Valid description";

        var result = Category.Create(name, description);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError!.Description.Should().Contain(NameResources.NameMaxLength);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update category data successfully when data is valid")]
    public void ShouldUpdateCategoryDataSuccessfullyWhenDataIsValid()
    {
        var name = Name.Create("Original Category");
        var category = Category.Create(name, "Original description").Data!;
        var newName = Name.Create("Updated Category");
        var newDescription = "Updated description";

        var result = category.UpdateCategory(newName, newDescription);

        result.Status.Should().Be(EResultStatus.Success);
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not load category when CreatedAt is not UTC")]
    public void ShouldNotLoadCategoryWhenCreatedAtIsNotUtc()
    {
        var id = 1L;
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.Now;

        var result = Category.Load(id, name, description, createdAt);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("CreatedAt must be UTC");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should load category with UpdatedAt successfully")]
    public void ShouldLoadCategoryWithUpdatedAtSuccessfully()
    {
        var id = 1L;
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

    [Theory]
    [InlineData(-1)]
    [Trait("Category", "Unit")]
    [DisplayName("Should not load category with negative ID")]
    public void ShouldNotLoadCategoryWithNegativeId(long invalidId)
    {
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.UtcNow;

        var result = Category.Load(invalidId, name, description, createdAt);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Id cannot be negative");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should load category with ID zero")]
    public void ShouldLoadCategoryWithIdZero()
    {
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.UtcNow;

        var result = Category.Load(0, name, description, createdAt);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(0);
    }
}
