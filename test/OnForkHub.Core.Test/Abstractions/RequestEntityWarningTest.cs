using OnForkHub.Core.Abstractions;

using System.Text.Json;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestEntityWarningTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should allow null ID")]
    public void ShouldAllowNullId()
    {
        var warning = new RequestEntityWarning("TestEntity", null, "Warning without ID");

        warning.Id.Should().BeNull();
        warning.Name.Should().Be("TestEntity");
        warning.Message.Should().Be("Warning without ID");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should be different when properties are different")]
    public void ShouldBeDifferentWhenPropertiesAreDifferent()
    {
        var warning1 = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");
        var warning2 = new RequestEntityWarning("AnotherEntity", 456, "Another warning");

        warning1.Should().NotBe(warning2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should be equal for instances with the same properties")]
    public void ShouldBeEqualForInstancesWithSameProperties()
    {
        var warning1 = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");
        var warning2 = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");

        warning1.Should().Be(warning2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create a new instance when modifying a property with 'with'")]
    public void ShouldCreateNewInstanceWhenModifyingWithWith()
    {
        var warning = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");

        var modifiedWarning = warning with { Message = "New warning" };

        modifiedWarning.Should().NotBeSameAs(warning);
        modifiedWarning.Message.Should().Be("New warning");
        warning.Message.Should().Be("Warning about the entity");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should deserialize from JSON correctly")]
    public void ShouldDeserializeFromJsonCorrectly()
    {
        /*lang=json,strict*/
        var json = "{\"Name\":\"TestEntity\",\"Id\":123,\"Message\":\"Warning about the entity\"}";

        var warning = JsonSerializer.Deserialize<RequestEntityWarning>(json);

        warning.Should().NotBeNull();
        warning!.Name.Should().Be("TestEntity");

        if (warning.Id is JsonElement idElement && idElement.ValueKind == JsonValueKind.Number)
        {
            warning = warning with { Id = idElement.GetInt64() };
        }

        warning.Id.Should().Be(123L);
        warning.Message.Should().Be("Warning about the entity");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should generate different hash codes for instances with different properties")]
    public void ShouldGenerateDifferentHashCodesForInstancesWithDifferentProperties()
    {
        var warning1 = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");
        var warning2 = new RequestEntityWarning("AnotherEntity", 456, "Another warning");

        warning1.GetHashCode().Should().NotBe(warning2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should generate the same hash code for instances with the same properties")]
    public void ShouldGenerateSameHashCodeForInstancesWithSameProperties()
    {
        var warning1 = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");
        var warning2 = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");

        warning1.GetHashCode().Should().Be(warning2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should initialize correctly with name, ID, and message")]
    public void ShouldInitializeCorrectlyWithNameIdAndMessage()
    {
        var name = "TestEntity";
        var id = 123;
        var message = "Warning about the entity";

        var warning = new RequestEntityWarning(name, id, message);

        warning.Name.Should().Be(name);
        warning.Id.Should().Be(id);
        warning.Message.Should().Be(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return correctly formatted string when calling ToString")]
    public void ShouldReturnCorrectlyFormattedStringWhenCallingToString()
    {
        var warning = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");

        var stringRepresentation = warning.ToString();

        stringRepresentation.Should().Contain("TestEntity");
        stringRepresentation.Should().Contain("123");
        stringRepresentation.Should().Contain("Warning about the entity");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return false when compared with null")]
    public void ShouldReturnFalseWhenComparedWithNull()
    {
        var warning = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");

        warning.Equals(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should serialize to JSON correctly")]
    public void ShouldSerializeToJsonCorrectly()
    {
        var warning = new RequestEntityWarning("TestEntity", 123, "Warning about the entity");

        var json = JsonSerializer.Serialize(warning);

        json.Should().Contain("\"Name\":\"TestEntity\"");
        json.Should().Contain("\"Id\":123");
        json.Should().Contain("\"Message\":\"Warning about the entity\"");
    }
}
