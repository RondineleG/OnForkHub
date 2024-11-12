namespace OnForkHub.Application.Test.Validation;

public class ValidationServiceTests
{
    private readonly BaseValidationService _baseValidationService;
    private readonly ICategoryValidationService _categoryValidationService;

    public ValidationServiceTests()
    {
        _baseValidationService = new TestValidationService();
        _categoryValidationService = new CategoryValidationService();
    }

    #region Base Validation Tests

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Base: Should validate string value")]
    public void BaseValidation_ShouldValidateStringValue()
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
    public void BaseValidation_ShouldAddErrorIfStringValueIsNullOrEmpty()
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
    public void BaseValidation_ShouldValidateDateRange()
    {
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var fieldName = "DateRange";

        var validationResult = _baseValidationService.Validate(startDate, endDate, fieldName);

        validationResult.IsValid.Should().BeTrue();
    }

    #endregion

    #region Category Validation Tests

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should validate valid category")]
    public void CategoryValidation_ShouldValidateValidCategory()
    {
        // Arrange
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        // Act
        var validationResult = _categoryValidationService.ValidateCategory(category);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should fail validation for null category")]
    public void CategoryValidation_ShouldFailValidationForNullCategory()
    {
        // Arrange
        Category? category = null;

        // Act
        var validationResult = _categoryValidationService.ValidateCategory(category!);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "Category cannot be null" && e.Field == nameof(Category));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should fail validation for long description")]
    public void CategoryValidation_ShouldFailValidationForLongDescription()
    {
        // Arrange
        var name = Name.Create("Test Category");
        var longDescription = new string('A', 201); // 201 characters
        var category = Category.Create(name, longDescription).Data!;

        // Act
        var validationResult = _categoryValidationService.ValidateCategory(category);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "Description cannot exceed 200 characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should validate category update with valid data")]
    public void CategoryValidation_ShouldValidateCategoryUpdateWithValidData()
    {
        // Arrange
        var name = Name.Create("Test Category");
        var category = Category.Load(1, name, "Test Description", DateTime.UtcNow).Data!;

        // Act
        var validationResult = _categoryValidationService.ValidateCategoryUpdate(category);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Category: Should fail validation for update without ID")]
    public void CategoryValidation_ShouldFailValidationForUpdateWithoutId()
    {
        // Arrange
        var name = Name.Create("Test Category");
        var category = Category.Create(name, "Test Description").Data!;

        // Act
        var validationResult = _categoryValidationService.ValidateCategoryUpdate(category);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "Category ID is required for updates" && e.Field == "Id");
    }

    #endregion

    #region Helper Classes

    private class TestValidationService : BaseValidationService { }

    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}

public class ValidationServiceFactoryTests
{
    private readonly IValidationServiceFactory _factory;

    public ValidationServiceFactoryTests()
    {
        _factory = new ValidationServiceFactory();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category validation service")]
    public void ShouldCreateCategoryValidationService()
    {
        // Act
        var service = _factory.CreateCategoryValidationService();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<ICategoryValidationService>();
    }
}
