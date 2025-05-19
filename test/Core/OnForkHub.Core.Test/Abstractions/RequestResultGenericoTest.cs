// The .NET Foundation licenses this file to you under the MIT license.

using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Test.Abstractions;

public class GenericRequestResultTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add entity errors in generic RequestResult")]
    public void ShouldAddEntityErrorsInGenericRequestResult()
    {
        var entityErrors = new Dictionary<string, List<string>>
        {
            {
                "TestEntity",
                new List<string> { "Entity error 1", "Entity error 2" }
            },
        };

        var result = RequestResult<string>.WithError(entityErrors);

        result.Status.Should().Be(EResultStatus.EntityHasError);
        result.EntityErrors.Should().ContainKey("TestEntity");
        result.EntityErrors["TestEntity"].Should().BeEquivalentTo(new List<string> { "Entity error 1", "Entity error 2" });
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should contain correct validation in validation list using implicit operator")]
    public void ShouldContainCorrectValidationInValidationListUsingImplicitOperator()
    {
        var validation = new RequestValidation("TestField", "Validation error for test field");

        RequestResult<string> result = validation;

        result.Validations.Should().ContainSingle(v => v.PropertyName == "TestField" && v.Description == "Validation error for test field");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should convert data to success result")]
    public void ShouldConvertDataToSuccessResult()
    {
        var data = "Test data";

        RequestResult<string> result = data;

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(data);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should convert exception to error result")]
    public void ShouldConvertExceptionToErrorResult()
    {
        var exception = new Exception("Test exception");

        RequestResult<string> result = exception;

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Be(exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should format error messages correctly in generic RequestResult")]
    public void ShouldFormatErrorMessagesCorrectlyInGenericRequestResult()
    {
        var result = new RequestResult<string>();
        result.AddError("General error 1");
        result.AddEntityError("Entity1", "Entity error 1");

        var formattedText = result.ToString();

        formattedText.Should().Contain("General error 1");
        formattedText.Should().Contain("Entity1: Entity error 1");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should initialize generic RequestResult without data")]
    public void ShouldInitializeGenericRequestResultWithoutData()
    {
        var result = new RequestResult<string>();

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().BeNull();
        result.EntityErrors.Should().NotBeNull().And.BeEmpty();
        result.GeneralErrors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should initialize validation list with a single entry using implicit operator")]
    public void ShouldInitializeValidationListWithSingleEntryUsingImplicitOperator()
    {
        var validation = new RequestValidation("Property", "Error message");

        RequestResult<string> result = validation;

        result.Validations.Should().HaveCount(1);
        result.Validations.First().PropertyName.Should().Be("Property");
        result.Validations.First().Description.Should().Be("Error message");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error with error object in generic RequestResult")]
    public void ShouldReturnErrorWithErrorObjectInGenericRequestResult()
    {
        var error = new RequestError("Specific test error");

        var result = RequestResult<string>.WithError(error);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().Be(error);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return RequestResult with EntityAlreadyExists status and configured RequestEntityWarning")]
    public void ShouldReturnRequestResultWithEntityAlreadyExistsStatusAndConfiguredRequestEntityWarning()
    {
        var entity = "Product";
        var id = 1;
        var description = "Product already exists";

        var result = RequestResult<string>.EntityAlreadyExists(entity, id, description);

        result.Status.Should().Be(EResultStatus.EntityAlreadyExists);
        result.RequestEntityWarning.Should().NotBeNull();
        result.RequestEntityWarning!.Name.Should().Be(entity);
        result.RequestEntityWarning.Id.Should().Be(id);
        result.RequestEntityWarning.Message.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return RequestResult with validation status using implicit operator")]
    public void ShouldReturnRequestResultWithValidationStatusUsingImplicitOperator()
    {
        var validation = new RequestValidation("Name", "Name is required");

        RequestResult<string> result = validation;

        result.Status.Should().Be(EResultStatus.HasValidation);
        result.Validations.Should().ContainSingle().Which.Should().Be(validation);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return result without content")]
    public void ShouldReturnResultWithoutContent()
    {
        var result = RequestResult<string>.WithNoContent();

        result.Status.Should().Be(EResultStatus.NoContent);
        result.Data.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return result with validation error message")]
    public void ShouldReturnResultWithValidationError()
    {
        var errorMessage = "Generic validation error";

        var result = RequestResult.WithValidationError(errorMessage);

        result.Status.Should().Be(EResultStatus.HasValidation);
        result.ValidationResult.Errors.Should().ContainSingle();
        result.ValidationResult.Errors.First().Message.Should().Be(errorMessage);
        result.ValidationResult.Errors.First().Field.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return result with validations")]
    public void ShouldReturnResultWithValidations()
    {
        var validation = new RequestValidation("TestProperty", "Test validation message");

        var result = RequestResult<string>.WithValidations(validation);

        result.Status.Should().Be(EResultStatus.HasValidation);
        result.Validations.Should().ContainSingle();
        result.Validations.First().Should().Be(validation);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return success with provided data")]
    public void ShouldReturnSuccessWithProvidedData()
    {
        var data = "Test data";

        var result = RequestResult<string>.Success(data);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(data);
    }
}
