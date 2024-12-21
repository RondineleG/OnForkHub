namespace OnForkHub.Core.Test.Validations;

public class ValidationErrorMessageTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create error message with field and message")]
    public void ShouldCreateErrorMessageWithFieldAndMessage()
    {
        var message = new ValidationErrorMessage("Error message", "Field");

        message.Message.Should().Be("Error message");
        message.Field.Should().Be("Field");
        message.Source.Should().BeEmpty();
        message.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create error message with source")]
    public void ShouldCreateErrorMessageWithSource()
    {
        var message = new ValidationErrorMessage("Error message", "Field", "Source");

        message.Source.Should().Be("Source");
        message.Field.Should().Be("Field");
        message.Message.Should().Be("Error message");
        message.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create error message with empty source when null")]
    public void ShouldCreateErrorMessageWithEmptySourceWhenNull()
    {
        var message = new ValidationErrorMessage("Error message", "Field", null);

        message.Source.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should serialize to JSON with all properties")]
    public void ShouldSerializeToJsonWithAllProperties()
    {
        var message = new ValidationErrorMessage("Error message", "Field", "Source");
        var json = message.ToString();

        json.Should().Contain("\"Field\":\"Field\"");
        json.Should().Contain("\"Message\":\"Error message\"");
        json.Should().Contain("\"Source\":\"Source\"");
        json.Should().Contain("\"Timestamp\":");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should use UTC timestamp")]
    public void ShouldUseUtcTimestamp()
    {
        var message = new ValidationErrorMessage("Error message", "Field");

        message.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }
}
