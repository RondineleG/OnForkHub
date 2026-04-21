namespace OnForkHub.Core.Test.Exceptions;

using FluentAssertions;
using OnForkHub.Core.Exceptions;
using System.Collections.ObjectModel;
using Xunit;

[Trait("Category", "Unit")]
public class ValidationExceptionTests
{
    [Fact]
    [Trait("Method", "Constructor")]
    public void ConstructorWithSingleErrorShouldInitializeCorrectly()
    {
        const string propertyName = "Email";
        const string errorMessage = "Invalid email format";

        var exception = new ValidationException(propertyName, errorMessage);

        exception.Should().NotBeNull();
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.HasErrors.Should().BeTrue();
        exception.ErrorCount.Should().Be(1);
        exception.GetErrors(propertyName).Should().Contain(errorMessage);
    }

    [Fact]
    [Trait("Method", "Constructor")]
    public void ConstructorWithMultipleErrorsShouldInitializeCorrectly()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Email",
                new List<string> { "Invalid email format", "Email is required" }
            },
            {
                "Password",
                new List<string> { "Password must be at least 8 characters" }
            },
        };

        var exception = new ValidationException(errors);

        exception.Should().NotBeNull();
        exception.HasErrors.Should().BeTrue();
        exception.ErrorCount.Should().Be(3);
        exception.GetErrors("Email").Should().HaveCount(2);
        exception.GetErrors("Password").Should().HaveCount(1);
    }

    [Fact]
    [Trait("Method", "GetErrors")]
    public void GetErrorsWithExistingPropertyShouldReturnErrorMessages()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Username",
                new List<string> { "Username is too short" }
            },
        };

        var exception = new ValidationException(errors);
        var result = exception.GetErrors("Username");

        result.Should().HaveCount(1);
        result.Should().Contain("Username is too short");
    }

    [Fact]
    [Trait("Method", "GetErrors")]
    public void GetErrorsWithNonexistentPropertyShouldReturnEmptyList()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Username",
                new List<string> { "Username is too short" }
            },
        };

        var exception = new ValidationException(errors);
        var result = exception.GetErrors("NonexistentProperty");

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Method", "AddError")]
    public void AddErrorShouldAddNewError()
    {
        var exception = new ValidationException("Email", "Invalid format");

        exception.AddError("Email", "Already exists");

        exception.ErrorCount.Should().Be(2);
        exception.GetErrors("Email").Should().HaveCount(2);
        exception.GetErrors("Email").Should().Contain("Already exists");
    }

    [Fact]
    [Trait("Method", "AddError")]
    public void AddErrorToNewPropertyShouldCreateProperty()
    {
        var exception = new ValidationException("Email", "Invalid format");

        exception.AddError("Password", "Too weak");

        exception.Errors.Should().HaveCount(2);
        exception.GetErrors("Password").Should().Contain("Too weak");
    }

    [Fact]
    [Trait("Method", "AddErrors")]
    public void AddErrorsShouldAddMultipleErrors()
    {
        var exception = new ValidationException("Email", "Invalid format");

        exception.AddErrors("Email", "Already exists", "Blacklisted");

        exception.ErrorCount.Should().Be(3);
        exception.GetErrors("Email").Should().HaveCount(3);
    }

    [Fact]
    [Trait("Method", "GetAllErrorsAsString")]
    public void GetAllErrorsAsStringShouldReturnFormattedString()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Email",
                new List<string> { "Invalid format", "Already exists" }
            },
            {
                "Password",
                new List<string> { "Too weak" }
            },
        };

        var exception = new ValidationException(errors);
        var result = exception.GetAllErrorsAsString();

        result.Should().Contain("Email: Invalid format");
        result.Should().Contain("Email: Already exists");
        result.Should().Contain("Password: Too weak");
    }

    [Fact]
    [Trait("Method", "Errors")]
    public void ErrorsShouldBeReadOnly()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Email",
                new List<string> { "Invalid format" }
            },
        };

        var exception = new ValidationException(errors);

        exception.Errors.Should().BeOfType<Dictionary<string, IReadOnlyList<string>>>();
    }

    [Fact]
    [Trait("Method", "ErrorCount")]
    public void ErrorCountWithMultipleErrorsShouldReturnCorrectCount()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Email",
                new List<string> { "Error 1", "Error 2" }
            },
            {
                "Password",
                new List<string> { "Error 3" }
            },
            {
                "Username",
                new List<string> { "Error 4", "Error 5" }
            },
        };

        var exception = new ValidationException(errors);

        exception.ErrorCount.Should().Be(5);
    }

    [Fact]
    [Trait("Method", "Constructor")]
    public void ConstructorWithCustomMessageShouldInitializeCorrectly()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Email",
                new List<string> { "Invalid email" }
            },
        };

        const string customMessage = "Registration validation failed";

        var exception = new ValidationException(customMessage, errors);

        exception.Message.Should().Be(customMessage);
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.HasErrors.Should().BeTrue();
    }

    [Fact]
    [Trait("Method", "Constructor")]
    public void ConstructorWithNullErrorsShouldHandleGracefully()
    {
        var errors = new Dictionary<string, List<string>>();
        var exception = new ValidationException("Test message", errors);

        exception.Errors.Should().BeEmpty();
        exception.HasErrors.Should().BeFalse();
    }
}
