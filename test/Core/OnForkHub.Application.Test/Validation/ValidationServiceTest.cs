using OnForkHub.Core.Validations.Base;

namespace OnForkHub.Application.Test.Validation;

public class ValidationServiceTest
{
    private readonly BaseValidationService _baseValidationService;
    private readonly CategoryValidationService _categoryValidationService;
    private readonly ValidationBuilder builder = new();

    public ValidationServiceTest()
    {
        _baseValidationService = new TestValidationService();
        _categoryValidationService = new CategoryValidationService(builder);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Base: Should validate string value")]
    public void BaseValidationShouldValidateStringValue()
    {
        var value = "test@example.com";
        var regexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        var fieldName = "Email";

        var validationResult = _baseValidationService.Validate(value, regexPattern, fieldName);

        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Base: Should add error if string value is null or empty")]
    public void BaseValidationShouldAddErrorIfStringValueIsNullOrEmpty()
    {
        var value = "";
        var regexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        var fieldName = "Email";

        var validationResult = _baseValidationService.Validate(value, regexPattern, fieldName);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "The field Email cannot be empty");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Base: Should validate date range")]
    public void BaseValidationShouldValidateDateRange()
    {
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var fieldName = "DateRange";

        var validationResult = _baseValidationService.Validate(startDate, endDate, fieldName);

        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should validate valid category")]
    public void CategoryValidationShouldValidateValidCategory()
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        var validationResult = _categoryValidationService.Validate(category);

        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should fail validation for long description")]
    public void CategoryValidationShouldFailValidationForLongDescription()
    {
        var name = Name.Create("Test Category");
        var longDescription = new string('A', 201);

        var category = new CategoryTestBuilder().WithName(name).WithDescription("Valid Description").Build();

        typeof(Category).GetProperty(nameof(Category.Description))!.SetValue(category, longDescription);

        var validationResult = _categoryValidationService.Validate(category);

        validationResult.IsValid.Should().BeFalse();
        validationResult
            .Errors.Should()
            .Contain(e => e.Message == "Description cannot exceed 200 characters" && e.Field == nameof(Category.Description));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should validate category update with valid data")]
    public void CategoryValidationShouldValidateCategoryUpdateWithValidData()
    {
        var name = Name.Create("Test Category");
        var category = Category.Load(1, name, "Test Description", DateTime.UtcNow).Data!;

        var validationResult = _categoryValidationService.Validate(category);

        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should fail validation for update without ID")]
    public void CategoryValidationShouldFailValidationForUpdateWithoutId()
    {
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        var validationResult = _categoryValidationService.ValidateUpdate(category);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "Category ID is required for updates" && e.Field == "Id");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should fail validation for null category")]
    public void CategoryValidationShouldFailValidationForNullCategory()
    {
        Category? category = null;

        var validationResult = _categoryValidationService.Validate(category!);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "Category cannot be null" && e.Field == nameof(Category));

        validationResult.Errors.Should().HaveCount(1);
    }
}
