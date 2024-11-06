using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Test.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not throw DomainException when condition is false")]
    public void ShouldNotThrowDomainExceptionWhenConditionIsFalse()
    {
        Action action = () => DomainException.ThrowErrorWhen(() => false, "Domain error");

        action.Should().NotThrow<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw DomainException when condition is true")]
    public void ShouldThrowDomainExceptionWhenConditionIsTrue()
    {
        var message = "Domain error";

        Action action = () => DomainException.ThrowErrorWhen(() => true, message);

        action.Should().Throw<DomainException>().WithMessage(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ThrowWhenInvalid should not throw exception when there are no errors")]
    public void ThrowWhenInvalidShouldNotThrowExceptionWhenNoErrors()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        Action action = () => DomainException.ThrowWhenInvalid(result1, result2);

        action.Should().NotThrow<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ThrowWhenInvalid should throw exception with messages for multiple errors")]
    public void ThrowWhenInvalidShouldThrowExceptionWithMessagesForMultipleErrors()
    {
        var result1 = ValidationResult.Failure("Error 1");
        var result2 = ValidationResult.Failure("Error 2");

        Action action = () => DomainException.ThrowWhenInvalid(result1, result2);

        action.Should().Throw<DomainException>().WithMessage("Error 1; Error 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ThrowWhenInvalid with single error should throw exception with error message")]
    public void ThrowWhenInvalidWithSingleErrorShouldThrowExceptionWithErrorMessage()
    {
        var result = ValidationResult.Failure("Single error");

        Action action = () => DomainException.ThrowWhenInvalid(result);

        action.Should().Throw<DomainException>().WithMessage("Single error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Validate should return failure when condition is true")]
    public void ValidateShouldReturnFailureWhenConditionIsTrue()
    {
        var message = "Domain error";
        var result = DomainException.Validate(() => true, message);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Validate should return success when condition is false")]
    public void ValidateShouldReturnSuccessWhenConditionIsFalse()
    {
        var message = "Domain error";
        var result = DomainException.Validate(() => false, message);

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }
}
