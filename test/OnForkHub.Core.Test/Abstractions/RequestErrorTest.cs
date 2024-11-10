using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestErrorTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should be different for instances with different descriptions")]
    public void ShouldBeDifferentForInstancesWithDifferentDescriptions()
    {
        var error1 = new RequestError("Validation error");
        var error2 = new RequestError("Another error");

        error1.Should().NotBe(error2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should be equal for instances with the same description")]
    public void ShouldBeEqualForInstancesWithSameDescription()
    {
        var error1 = new RequestError("Validation error");
        var error2 = new RequestError("Validation error");

        error1.Should().Be(error2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should deserialize from JSON correctly")]
    public void ShouldDeserializeFromJsonCorrectly()
    {
        var json = //
            "{\"Description\":\"Validation error\"}";

        var error = JsonSerializer.Deserialize<RequestError>(json);

        error.Should().NotBeNull();
        error!.Description.Should().Be("Validation error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should generate different hash codes for instances with different descriptions")]
    public void ShouldGenerateDifferentHashCodesForInstancesWithDifferentDescriptions()
    {
        var error1 = new RequestError("Validation error");
        var error2 = new RequestError("Another error");

        error1.GetHashCode().Should().NotBe(error2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should generate the same hash code for instances with the same description")]
    public void ShouldGenerateSameHashCodeForInstancesWithSameDescription()
    {
        var error1 = new RequestError("Validation error");
        var error2 = new RequestError("Validation error");

        error1.GetHashCode().Should().Be(error2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should initialize correctly with description")]
    public void ShouldInitializeCorrectlyWithDescription()
    {
        var description = "Validation error";

        var error = new RequestError(description);

        error.Description.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should maintain immutability when creating a modified copy")]
    public void ShouldMaintainImmutabilityWhenCreatingModifiedCopy()
    {
        var error = new RequestError("Validation error");

        var modifiedError = error with { Description = "New error" };

        modifiedError.Should().NotBeSameAs(error);
        modifiedError.Description.Should().Be("New error");
        error.Description.Should().Be("Validation error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return correctly formatted string when calling ToString")]
    public void ShouldReturnCorrectlyFormattedStringWhenCallingToString()
    {
        var error = new RequestError("Validation error");

        var stringRepresentation = error.ToString();

        stringRepresentation.Should().Contain("Validation error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return false when compared with null")]
    public void ShouldReturnFalseWhenComparedWithNull()
    {
        var error = new RequestError("Validation error");

        error.Equals(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should serialize to JSON correctly")]
    public void ShouldSerializeToJsonCorrectly()
    {
        var error = new RequestError("Validation error");

        var json = JsonSerializer.Serialize(error);

        json.Should().Contain("\"Description\":\"Validation error\"");
    }
}
