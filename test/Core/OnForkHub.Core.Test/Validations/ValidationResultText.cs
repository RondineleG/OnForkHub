namespace OnForkHub.Core.Test.Validations;

public class ValidationResultTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when condition is true")]
    public void ShouldAddErrorWhenConditionIsTrue()
    {
        var result = new ValidationResult().AddErrorIf(() => true, "Condition error", "Field");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Be("Condition error");
        result.Errors.First().Field.Should().Be("Field");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when string is empty")]
    public void ShouldAddErrorWhenStringIsEmpty()
    {
        var value = string.Empty;

        var result = new ValidationResult().AddErrorIf(() => string.IsNullOrEmpty(value), "The value cannot be empty", "Field");

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Field: The value cannot be empty");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when string is only spaces")]
    public void ShouldAddErrorWhenStringIsOnlySpaces()
    {
        var value = "   ";
        var result = new ValidationResult().AddErrorIf(() => string.IsNullOrWhiteSpace(value), "The value cannot be blank", "Field");

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Field: The value cannot be blank");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when value is null")]
    public void ShouldAddErrorWhenValueIsNull()
    {
        string? nullValue = null;
        var result = new ValidationResult().AddErrorIf(() => nullValue is null, "The value cannot be null", "Field");

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Field: The value cannot be null");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error with source")]
    public void ShouldAddErrorWithSource()
    {
        var result = new ValidationResult().AddError("Error message", "Field", "SourceTest");

        result.Errors.First().Source.Should().Be("SourceTest");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should combine validations with error")]
    public void ShouldCombineValidationsWithError()
    {
        var result1 = ValidationResult.Failure("Error 1");
        var result2 = ValidationResult.Failure("Error 2");

        var combinedResult = ValidationResult.Combine(result1, result2);

        combinedResult.IsValid.Should().BeFalse();
        combinedResult.ErrorMessage.Should().Be("Error 1; Error 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should handle validation with all components")]
    public void ShouldHandleCompleteValidation()
    {
        var result = ValidationResult.Success().AddError("Error 1", "Field1", "Source1").AddErrorIf(() => true, "Error 2", "Field2");

        result.Errors.Should().HaveCount(2);
        result.Errors.First().Source.Should().Be("Source1");
        result.ErrorMessage.Should().Contain("Field1: Error 1");
        result.ErrorMessage.Should().Contain("Field2: Error 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should correctly handle metadata operations")]
    public void ShouldHandleMetadataOperations()
    {
        var result = ValidationResult.Success();
        var testObject = new { Name = "Test" };
        result.Metadata["test"] = testObject;

        var retrieved = result.GetMetadata<object>("test");

        retrieved.Should().NotBeNull();
        result.Metadata.Should().ContainKey("test");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should handle OR operator correctly")]
    public void ShouldHandleOrOperatorCorrectly()
    {
        var success = ValidationResult.Success();
        var failure = ValidationResult.Failure("Error");

        var result = success | failure;

        result.Should().Be(success);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should keep errors when merging with another error result")]
    public void ShouldKeepErrorsWhenMergingWithAnotherErrorResult()
    {
        var result1 = ValidationResult.Failure("Error 1");
        var result2 = ValidationResult.Failure("Error 2");

        result1.Merge(result2);

        result1.IsValid.Should().BeFalse();
        result1.Errors.Count.Should().Be(2);
        result1.ErrorMessage.Should().Be("Error 1; Error 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should keep success when merging with another success")]
    public void ShouldKeepSuccessWhenMergingWithAnotherSuccess()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        result1.Merge(result2);

        result1.IsValid.Should().BeTrue();
        result1.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not add error when condition is false")]
    public void ShouldNotAddErrorWhenConditionIsFalse()
    {
        var result = new ValidationResult().AddErrorIf(() => false, "Condition error", "Field");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not throw exception when valid")]
    public void ShouldNotThrowExceptionWhenValid()
    {
        var result = ValidationResult.Success();

        var action = () => result.ThrowIfInvalid();

        action.Should().NotThrow<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error with specific field when using Failure")]
    public void ShouldReturnErrorWithSpecificFieldWhenUsingFailure()
    {
        var result = ValidationResult.Failure("Specific error", "SpecificField");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Field.Should().Be("SpecificField");
        result.ErrorMessage.Should().Be("SpecificField: Specific error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return failure when calling Failure")]
    public void ShouldReturnFailureWhenCallingFailure()
    {
        var errorMessage = "Validation error";
        var result = ValidationResult.Failure(errorMessage);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return success when calling Success")]
    public void ShouldReturnSuccessWhenCallingSuccess()
    {
        var result = ValidationResult.Success();

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw custom message when invalid")]
    public void ShouldThrowCustomMessageWhenInvalid()
    {
        var result = ValidationResult.Failure("Original error");
        var customMessage = "Custom error message";

        var action = () => result.ThrowIfInvalid(customMessage);

        action.Should().Throw<DomainException>().WithMessage(customMessage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception with message when invalid")]
    public void ShouldThrowExceptionWithMessageWhenInvalid()
    {
        var errorMessage = "Validation error";
        var result = ValidationResult.Failure(errorMessage);

        var action = () => result.ThrowIfInvalid();

        action.Should().Throw<DomainException>().WithMessage(errorMessage);
    }

    [Theory]
    [InlineData(false, "Validation error", false)]
    [InlineData(true, "", true)]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate condition correctly")]
    public void ShouldValidateConditionCorrectly(bool condition, string message, bool expectedValid)
    {
        var result = new ValidationResult().AddErrorIf(() => !condition, message);

        result.IsValid.Should().Be(expectedValid);
        if (!expectedValid)
        {
            result.ErrorMessage.Should().Be(message);
        }
    }
}