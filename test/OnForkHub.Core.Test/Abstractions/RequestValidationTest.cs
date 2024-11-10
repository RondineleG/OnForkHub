using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestValidationTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should allow null values for properties")]
    public void ShouldAllowNullValuesForProperties()
    {
        var validation = new RequestValidation(null, null);

        validation.PropertyName.Should().BeNull();
        validation.Description.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should be considered different from another type when using Equals")]
    public void ShouldBeConsideredDifferentFromAnotherTypeWhenUsingEquals()
    {
        var validation = new RequestValidation("TestField", "Description");

        validation.Equals("OtherType").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should be different when one property is null")]
    public void ShouldBeDifferentWhenOnePropertyIsNull()
    {
        var validation1 = new RequestValidation("TestField", null);
        var validation2 = new RequestValidation("TestField", "Description");

        validation1.Should().NotBe(validation2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create a new RequestValidation instance with 'with' to change description")]
    public void ShouldCreateNewInstanceWithWithToChangeDescription()
    {
        var validation = new RequestValidation("Name", "Name is required");

        var updatedValidation = validation with { Description = "Name must have at least 3 characters" };

        updatedValidation.Should().NotBeSameAs(validation);
        updatedValidation.PropertyName.Should().Be("Name");
        updatedValidation.Description.Should().Be("Name must have at least 3 characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create a new RequestValidation instance with 'with' to change property name")]
    public void ShouldCreateNewInstanceWithWithToChangePropertyName()
    {
        var validation = new RequestValidation("Name", "Name is required");

        var updatedValidation = validation with { PropertyName = "Age" };

        updatedValidation.Should().NotBeSameAs(validation);
        updatedValidation.PropertyName.Should().Be("Age");
        updatedValidation.Description.Should().Be("Name is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create RequestValidation with properties correctly set")]
    public void ShouldCreateRequestValidationWithPropertiesCorrectlySet()
    {
        var propertyName = "Name";
        var description = "Name is required";

        var validation = new RequestValidation(propertyName, description);

        validation.PropertyName.Should().Be(propertyName);
        validation.Description.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should deserialize from JSON correctly")]
    public void ShouldDeserializeFromJsonCorrectly()
    {
        var json = "{\"PropertyName\":\"TestField\",\"Description\":\"Error description\"}";
        var validation = JsonSerializer.Deserialize<RequestValidation>(json);

        validation.Should().NotBeNull();
        validation!.PropertyName.Should().Be("TestField");
        validation.Description.Should().Be("Error description");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should initialize correctly with property name and description")]
    public void ShouldInitializeCorrectlyWithPropertyNameAndDescription()
    {
        var propertyName = "TestField";
        var description = "Validation error description";
        var validation = new RequestValidation(propertyName, description);

        validation.PropertyName.Should().Be(propertyName);
        validation.Description.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return equivalent instance when 'with' operator used without modifications")]
    public void ShouldReturnEquivalentInstanceWhenWithOperatorUsedWithoutModifications()
    {
        var validation = new RequestValidation("TestField", "Description");
        var validationCopy = validation with { };

        validationCopy.Should().Be(validation);
        validationCopy.Should().NotBeSameAs(validation);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return false when comparing instance with null")]
    public void ShouldReturnFalseWhenComparingInstanceWithNull()
    {
        var validation = new RequestValidation("TestField", "Description");

        validation.Equals(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return formatted string correctly when calling ToString")]
    public void ShouldReturnFormattedStringCorrectlyWhenCallingToString()
    {
        var validation = new RequestValidation("TestField", "Error description");
        var stringRepresentation = validation.ToString();

        stringRepresentation.Should().Contain("TestField");
        stringRepresentation.Should().Contain("Error description");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should serialize to JSON correctly")]
    public void ShouldSerializeToJsonCorrectly()
    {
        var validation = new RequestValidation("TestField", "Error description");
        var json = JsonSerializer.Serialize(validation);

        var deserializedValidation = JsonSerializer.Deserialize<RequestValidation>(json);

        deserializedValidation.Should().NotBeNull();
        deserializedValidation!.PropertyName.Should().Be("TestField");
        deserializedValidation.Description.Should().Be("Error description");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should support explicit comparison using Equals")]
    public void ShouldSupportExplicitComparisonUsingEquals()
    {
        var validation1 = new RequestValidation("TestField", "Description");
        var validation2 = new RequestValidation("TestField", "Description");

        validation1.Equals(validation2).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Two RequestValidation instances with different values should be different")]
    public void TwoRequestValidationInstancesWithDifferentValuesShouldBeDifferent()
    {
        var validation1 = new RequestValidation("Name", "Name is required");
        var validation2 = new RequestValidation("Age", "Age is required");

        validation1.Should().NotBe(validation2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Two RequestValidation instances with same values should be equal")]
    public void TwoRequestValidationInstancesWithSameValuesShouldBeEqual()
    {
        var validation1 = new RequestValidation("Name", "Name is required");
        var validation2 = new RequestValidation("Name", "Name is required");

        validation1.Should().Be(validation2);
    }
}
