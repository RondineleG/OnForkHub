using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Test.Validations;

public class ValidationResultTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when condition is true")]
    public void ShouldAddErrorWhenConditionIsTrue()
    {
        var result = new ValidationResult().AddErrorIf(true, "Condition error", "Field");

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
        var result = new ValidationResult().AddErrorIfNullOrEmpty(value, "The value cannot be empty", "Field");

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("The value cannot be empty");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when string is only spaces")]
    public void ShouldAddErrorWhenStringIsOnlySpaces()
    {
        var value = "   ";
        var result = new ValidationResult().AddErrorIfNullOrWhiteSpace(
            value,
            "The value cannot be blank",
            "Field"
        );

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("The value cannot be blank");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when value is null")]
    public void ShouldAddErrorWhenValueIsNull()
    {
        string? nullValue = null;
        var result = new ValidationResult().AddErrorIfNull(nullValue, "The value cannot be null", "Field");

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("The value cannot be null");
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
        var result = new ValidationResult().AddErrorIf(false, "Condition error", "Field");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not add error when value is not null")]
    public void ShouldNotAddErrorWhenValueIsNotNull()
    {
        var value = "text";
        var result = new ValidationResult().AddErrorIfNull(value, "The value cannot be null", "Field");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not throw exception when valid")]
    public void ShouldNotThrowExceptionWhenValid()
    {
        var result = ValidationResult.Success();

        Action action = result.ThrowIfInvalid;

        action.Should().NotThrow<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error with multiple fields when adding errors")]
    public void ShouldReturnErrorWithMultipleFieldsWhenAddingErrors()
    {
        var errors = new List<(string Message, string Field)>
        {
            ("Error in field 1", "Field1"),
            ("Error in field 2", "Field2"),
        };
        var result = new ValidationResult().AddErrors(errors);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(2);
        result.Errors.Select(e => e.Message).Should().Contain("Error in field 1").And.Contain("Error in field 2");
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
        result.ErrorMessage.Should().Be("Specific error");
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
    [DisplayName("Should return first failure when using AND operator")]
    public void ShouldReturnFirstFailureWhenUsingAndOperator()
    {
        var result1 = ValidationResult.Failure("Error 1");
        var result2 = ValidationResult.Failure("Error 2");

        var finalResult = result1 & result2;

        finalResult.IsValid.Should().BeFalse();
        finalResult.ErrorMessage.Should().Be("Error 1");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return success when all validations are success")]
    public void ShouldReturnSuccessWhenAllValidationsAreSuccess()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        var combinedResult = ValidationResult.Combine(result1, result2);

        combinedResult.IsValid.Should().BeTrue();
        combinedResult.ErrorMessage.Should().BeEmpty();
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
    [DisplayName("Should return success when using AND operator with all valid")]
    public void ShouldReturnSuccessWhenUsingAndOperatorWithAllValid()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        var finalResult = result1 & result2;

        finalResult.IsValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when attempting to merge with null")]
    public void ShouldThrowExceptionWhenAttemptingToMergeWithNull()
    {
        var result = new ValidationResult();
        ValidationResult? validate = null;

        Action action = () => result.Merge(validate);

        action.Should().Throw<ArgumentNullException>().WithMessage("*other*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception with message when invalid")]
    public void ShouldThrowExceptionWithMessageWhenInvalid()
    {
        var errorMessage = "Validation error";
        var result = ValidationResult.Failure(errorMessage);

        Action action = result.ThrowIfInvalid;

        action.Should().Throw<DomainException>().WithMessage(errorMessage);
    }

    [Theory]
    [InlineData(false, "Validation error", false)]
    [InlineData(true, "", true)]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate condition correctly")]
    public void ShouldValidateConditionCorrectly(bool condition, string message, bool expectedValid)
    {
        var result = ValidationResult.Validate(() => !condition, message);

        result.IsValid.Should().Be(expectedValid);
        if (!expectedValid)
        {
            result.ErrorMessage.Should().Be(message);
        }
    }
}
