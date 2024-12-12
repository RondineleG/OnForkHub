namespace OnForkHub.Core.Test.Validations;

public class ValidationBuilderTest
{
    private readonly ValidationBuilder<TestEntity> _builder = new();

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set current field and validate successfully")]
    public void WithFieldShouldSetCurrentField()
    {
        var result = _builder.WithField("Name", "Test").NotNull().Validate();

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when value is null")]
    public void NotNullShouldAddErrorWhenValueIsNull()
    {
        var result = _builder.WithField("Name").NotNull("Name is required").Validate();

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Name: Name is required");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when string is empty or null")]
    [InlineData("")]
    [InlineData(null)]
    public void NotEmptyShouldAddErrorWhenStringIsEmpty(string? value)
    {
        var result = _builder.WithField("Name", value).NotEmpty().Validate();

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when string contains only whitespace")]
    public void NotWhiteSpaceShouldAddErrorWhenStringIsWhiteSpace()
    {
        var result = _builder.WithField("Name", "   ").NotWhiteSpace().Validate();

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when string length is below minimum")]
    [InlineData("ab", 3)]
    [InlineData("a", 2)]
    public void MinLengthShouldAddErrorWhenStringIsTooShort(string value, int minLength)
    {
        var result = _builder.WithField("Name", value).MinLength(minLength).Validate();

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when string length exceeds maximum")]
    [InlineData("abc", 2)]
    [InlineData("abcd", 3)]
    public void MaxLengthShouldAddErrorWhenStringIsTooLong(string value, int maxLength)
    {
        var result = _builder.WithField("Name", value).MaxLength(maxLength).Validate();

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when string length does not match exact length")]
    public void LengthShouldAddErrorWhenStringLengthDoesNotMatch()
    {
        var result = _builder.WithField("Name", "abc").Length(4).Validate();

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when value is outside specified range")]
    public void RangeShouldAddErrorWhenValueOutsideRange()
    {
        var result = _builder.WithField("Age", 15).Range(18, 65).Validate();

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when string does not match pattern")]
    public void MatchesShouldAddErrorWhenPatternDoesNotMatch()
    {
        var result = _builder.WithField("Email", "invalid-email").Matches(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$").Validate();

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when custom validation fails")]
    public void CustomShouldAddErrorWhenValidationFails()
    {
        var result = _builder.WithField("Value", 13).Custom(value => (int?)value % 2 == 0, "Value must be even").Validate();

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when async custom validation fails")]
    public async Task CustomAsyncShouldAddErrorWhenAsyncValidationFails()
    {
        var builder = new ValidationBuilder<TestEntity>();

        await builder
            .WithField("Value", "test")
            .CustomAsync(
                async value =>
                {
                    await Task.Delay(1);
                    return false;
                },
                "Async validation failed"
            );

        var result = builder.Validate();
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add metadata with specified key and value")]
    public void WithMetadataShouldAddMetadata()
    {
        var result = _builder.WithField("Name", "Test").WithMetadata("key", "value").Validate();

        result.Metadata["key"].Should().Be("value");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when ensure predicate fails")]
    public void EnsureShouldAddErrorWhenPredicateFails()
    {
        var result = _builder.WithField("Value", 10).Ensure(() => false, "Predicate failed").Validate();

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add error when async ensure predicate fails")]
    public async Task EnsureAsyncShouldAddErrorWhenAsyncPredicateFails()
    {
        var builder = new ValidationBuilder<TestEntity>();

        await builder
            .WithField("Value", "test")
            .EnsureAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return false;
                },
                "Async predicate failed"
            );

        var result = builder.Validate();
        result.IsValid.Should().BeFalse();
    }

    private sealed class TestEntity : BaseEntity
    {
    }
}