namespace OnForkHub.Application.Test.Validation;

public class ValidationServiceTest
{
    private readonly ValidationService _validationService;

    public ValidationServiceTest()
    {
        _validationService = new ValidationService();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate string value")]
    public void ShouldValidateStringValue()
    {
        var value = "test@example.com";
        var regexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        var fieldName = "Email";

        var validationResult = _validationService.Validate(value, regexPattern, fieldName);

        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error if string value is null or empty")]
    public void ShouldAddErrorIfStringValueIsNullOrEmpty()
    {
        var value = "";
        var regexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        var fieldName = "Email";

        var validationResult = _validationService.Validate(value, regexPattern, fieldName);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "The field Email cannot be empty");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error if string value does not match regex pattern")]
    public void ShouldAddErrorIfStringValueDoesNotMatchRegexPattern()
    {
        var value = "invalid-email";
        var regexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        var fieldName = "Email";

        var validationResult = _validationService.Validate(value, regexPattern, fieldName);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "The field Email does not match the required pattern");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error if regex pattern is invalid")]
    public void ShouldAddErrorIfRegexPatternIsInvalid()
    {
        var value = "test@example.com";
        var regexPattern = "[";
        var fieldName = "Email";

        var validationResult = _validationService.Validate(value, regexPattern, fieldName);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "Invalid regex pattern for field Email");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate date range")]
    public void ShouldValidateDateRange()
    {
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var fieldName = "DateRange";

        var validationResult = _validationService.Validate(startDate, endDate, fieldName);

        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error if start date is default")]
    public void ShouldAddErrorIfStartDateIsDefault()
    {
        var startDate = default(DateTime);
        var endDate = new DateTime(2023, 12, 31);
        var fieldName = "DateRange";

        var validationResult = _validationService.Validate(startDate, endDate, fieldName);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "The start date for DateRange cannot be empty");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error if end date is default")]
    public void ShouldAddErrorIfEndDateIsDefault()
    {
        var startDate = new DateTime(2023, 1, 1);
        var endDate = default(DateTime);
        var fieldName = "DateRange";

        var validationResult = _validationService.Validate(startDate, endDate, fieldName);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "The end date for DateRange cannot be empty");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error if start date is after end date")]
    public void ShouldAddErrorIfStartDateIsAfterEndDate()
    {
        var startDate = new DateTime(2023, 12, 31);
        var endDate = new DateTime(2023, 1, 1);
        var fieldName = "DateRange";

        var validationResult = _validationService.Validate(startDate, endDate, fieldName);

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.Message == "The start date for DateRange must be before the end date");
    }

    private static CustomValidationResult ValidateTestEntity(TestEntity entity)
    {
        var validationResult = new CustomValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(entity.Name, "Name is required", nameof(entity.Name));
        return validationResult;
    }

    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}
