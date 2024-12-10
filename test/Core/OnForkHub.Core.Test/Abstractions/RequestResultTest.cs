using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestResultTest
{
    private static readonly string[] expected = ["General error 1", "General error 2"];

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add entity error")]
    public void ShouldAddEntityError()
    {
        var result = new RequestResult();
        var entity = "TestEntity";
        var message = "Test entity error message";

        result.AddEntityError(entity, message);

        result.Status.Should().Be(EResultStatus.EntityHasError);
        result.EntityErrors.Should().ContainKey(entity);
        result.EntityErrors[entity].Should().Contain(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error with list of errors and exception")]
    public void ShouldAddErrorWithListAndException()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var exception = new InvalidOperationException("Exception error");
        var result = RequestResult.WithError(errors);
        result.AddError(exception.Message);

        result.Status.Should().Be(EResultStatus.HasError);
        result.GeneralErrors.Should().Contain(["Error 1", "Error 2", exception.Message]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add multiple errors for the same entity")]
    public void ShouldAddMultipleErrorsForSameEntity()
    {
        string[] expected = ["Entity error 1", "Entity error 2"];
        var result = new RequestResult();
        var entity = "TestEntity";

        result.AddEntityError(entity, "Entity error 1");
        result.AddEntityError(entity, "Entity error 2");

        result.Status.Should().Be(EResultStatus.EntityHasError);
        result.EntityErrors.Should().ContainKey(entity);
        result.EntityErrors[entity].Should().Contain(expected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add multiple general errors")]
    public void ShouldAddMultipleGeneralErrors()
    {
        var result = new RequestResult();
        result.AddError("General error 1");
        result.AddError("General error 2");

        result.Status.Should().Be(EResultStatus.HasError);
        result.GeneralErrors.Should().HaveCount(2);
        result.GeneralErrors.Should().Contain(expected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add validations as a collection")]
    public void ShouldAddValidationsAsCollection()
    {
        var validations = new List<RequestValidation> { new("Field1", "Validation error 1"), new("Field2", "Validation error 2") };

        var result = RequestResult.WithValidations(validations.ToArray());

        result.Status.Should().Be(EResultStatus.HasValidation);
        result.ValidationResult.Errors.Should().HaveCount(2);
        result
            .ValidationResult.Errors.Should()
            .Contain(error => error.Message == "Validation error 1" && error.Field == "Field1")
            .And.Contain(error => error.Message == "Validation error 2" && error.Field == "Field2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add validation with property and description")]
    public void ShouldAddValidationWithPropertyAndDescription()
    {
        var result = RequestResult.WithValidations(new RequestValidation("TestProperty", "Test validation error"));

        result.Status.Should().Be(EResultStatus.HasValidation);

        result.ValidationResult.Errors.Should().ContainSingle();

        var error = result.ValidationResult.Errors.First();
        error.Field.Should().Be("TestProperty");
        error.Message.Should().Be("Test validation error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should format error messages correctly")]
    public void ShouldFormatErrorMessagesCorrectly()
    {
        var result = new RequestResult();
        result.AddError("General error 1");
        result.AddError("General error 2");
        result.AddEntityError("Entity1", "Entity error 1");
        result.AddEntityError("Entity1", "Entity error 2");

        var formattedText = result.ToString();

        formattedText.Should().Contain("General error 1");
        formattedText.Should().Contain("General error 2");
        formattedText.Should().Contain("Entity1: Entity error 1");
        formattedText.Should().Contain("Entity1: Entity error 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should format validation messages in ToString method")]
    public void ShouldFormatValidationMessagesInToStringMethod()
    {
        var result = RequestResult.WithValidations(
            new RequestValidation("TestField", "Validation error 1"),
            new RequestValidation("TestField", "Validation error 2")
        );

        var formattedText = result.ToString();

        formattedText.Should().Contain("TestField: Validation error 1");
        formattedText.Should().Contain("TestField: Validation error 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should initialize error fields only when accessed")]
    public void ShouldInitializeErrorFieldsOnlyWhenAccessed()
    {
        var result = new RequestResult();

        result.EntityErrors.Should().BeEmpty();
        result.GeneralErrors.Should().BeEmpty();

        result.EntityErrors.Should().NotBeNull();
        result.GeneralErrors.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should initialize with success status")]
    public void ShouldInitializeWithSuccessStatus()
    {
        var result = new RequestResult();

        result.Status.Should().Be(EResultStatus.Success);
        result.ValidationResult.Should().NotBeNull();
        result.EntityErrors.Should().NotBeNull().And.BeEmpty();
        result.GeneralErrors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return entity not found result")]
    public void ShouldReturnEntityNotFoundResult()
    {
        var entity = "TestEntity";
        var id = 1;
        var message = "Entity not found";

        var result = RequestResult.EntityNotFound(entity, id, message);

        result.Status.Should().Be(EResultStatus.EntityNotFound);
        result.RequestEntityWarning.Should().NotBeNull();
        result.RequestEntityWarning!.Name.Should().Be(entity);
        result.RequestEntityWarning.Id.Should().Be(id);
        result.RequestEntityWarning.Message.Should().Be(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error result when list of errors is provided")]
    public void ShouldReturnErrorResultWhenErrorListProvided()
    {
        var errors = new List<string> { "Error 1", "Error 2" };

        var result = RequestResult.WithError(errors);

        result.Status.Should().Be(EResultStatus.HasError);
        result.GeneralErrors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error result when exception is provided")]
    public void ShouldReturnErrorResultWhenExceptionProvided()
    {
        var exception = new Exception("Test exception");

        var result = RequestResult.WithError(exception);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Be(exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error result when message is provided")]
    public void ShouldReturnErrorResultWhenMessageProvided()
    {
        var errorMessage = "Test error message";

        var result = RequestResult.WithError(errorMessage);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Be(errorMessage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error with custom error object")]
    public void ShouldReturnErrorWithCustomErrorObject()
    {
        var customError = new RequestError("Custom error");

        var result = RequestResult.WithError(customError);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().Be(customError);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return result without content")]
    public void ShouldReturnResultWithoutContent()
    {
        var result = RequestResult.WithNoContent();

        result.Status.Should().Be(EResultStatus.NoContent);
        result.EntityErrors.Should().BeEmpty();
        result.GeneralErrors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return validation error result")]
    public void ShouldReturnValidationErrorResult()
    {
        var errorMessage = "Validation error";
        var fieldName = "TestField";

        var result = RequestResult.WithValidationError(errorMessage, fieldName);

        result.Status.Should().Be(EResultStatus.HasValidation);
        result.ValidationResult.Errors.Should().ContainSingle();
        result.ValidationResult.Errors.First().Message.Should().Be(errorMessage);
        result.ValidationResult.Errors.First().Field.Should().Be(fieldName);
    }
}
