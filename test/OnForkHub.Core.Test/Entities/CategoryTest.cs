namespace OnForkHub.Core.Test.Entities;

public class CategoryTests
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
        ;
        var description = "Category description";
        var createdAt = DateTime.Now;

        var result = Category.Load(id, name, description, createdAt);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(id);
        result.Data.Name.Should().Be(name);
        result.Data.Description.Should().Be(description);
        result.Data.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw DomainException when creating category with empty name")]
    public void ShouldThrowDomainExceptionWhenCreatingCategoryWithEmptyName()
    {
        var name = "";
        var description = "Category description";

        Action action = () => Category.Create(Name.Create(name), description);

        action.Should().Throw<DomainException>().WithMessage("Name cannot be empty or null");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when loading category with invalid ID")]
    public void ShouldReturnErrorWhenLoadingCategoryWithInvalidId()
    {
        var id = 0L;
        var name = Name.Create("Category Test");
        var description = "Category description";
        var createdAt = DateTime.Now;

        Action act = () => Category.Load(id, name, description, createdAt);
        act.Should().Throw<DomainException>().WithMessage("Id must be greater than zero");
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
        result.RequestError!.Description.Should().Contain("Name must be no more than 50 characters");
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
    [DisplayName("Should validate category correctly")]
    public void ShouldValidateCategoryCorrectly()
    {
        var name = Name.Create("Valid Category");

        var category = Category.Create(name, "Valid description").Data!;

        var validationResult = category.Validate();

        validationResult.Errors.Should().BeEmpty();
    }
}
