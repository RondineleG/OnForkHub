using OnForkHub.Core.Extensions;

namespace OnForkHub.Core.Test.Enums;

public class EnumExtensionsTest
{
    public enum ETestResultStatus
    {
        [Description("Successful Result")]
        Success,

        [Description("Has Validation Errors")]
        HasValidation,

        NoDescription,
    }

    [Theory]
    [InlineData(EResultStatus.Success, "Success")]
    [InlineData(EResultStatus.HasValidation, "HasValidation")]
    [InlineData(EResultStatus.HasError, "HasError")]
    [InlineData(EResultStatus.EntityNotFound, "EntityNotFound")]
    [InlineData(EResultStatus.EntityHasError, "EntityHasError")]
    [InlineData(EResultStatus.EntityAlreadyExists, "EntityAlreadyExists")]
    [InlineData(EResultStatus.NoContent, "NoContent")]
    [Trait("Category", "Unit")]
    [DisplayName("Should return correct description for EResultStatus")]
    public void GetDescriptionShouldReturnCorrectValueForEResultStatus(EResultStatus status, string expected)
    {
        var description = status.GetDescription();
        description.Should().Be(expected);
    }

    [Theory]
    [InlineData(ETestResultStatus.Success, "Successful Result")]
    [InlineData(ETestResultStatus.HasValidation, "Has Validation Errors")]
    [InlineData(ETestResultStatus.NoDescription, "NoDescription")]
    [Trait("Category", "Unit")]
    [DisplayName("Should return description attribute or enum name")]
    public void GetDescriptionShouldReturnDescriptionAttributeOrEnumName(ETestResultStatus status, string expected)
    {
        var description = status.GetDescription();
        description.Should().Be(expected);
    }

    [Theory]
    [InlineData("Success", EResultStatus.Success)]
    [InlineData("HasValidation", EResultStatus.HasValidation)]
    [InlineData("HasError", EResultStatus.HasError)]
    [InlineData("EntityNotFound", EResultStatus.EntityNotFound)]
    [InlineData("EntityHasError", EResultStatus.EntityHasError)]
    [InlineData("EntityAlreadyExists", EResultStatus.EntityAlreadyExists)]
    [InlineData("NoContent", EResultStatus.NoContent)]
    [Trait("Category", "Unit")]
    [DisplayName("Should parse enum from valid description")]
    public void ParseFromDescriptionShouldReturnCorrectEnum(string description, EResultStatus expected)
    {
        var result = EnumExtensions.ParseFromDescription<EResultStatus>(description);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("SUCCESSFUL RESULT")]
    [InlineData("successful result")]
    [InlineData("Successful Result")]
    [Trait("Category", "Unit")]
    [DisplayName("Should parse enum from description case insensitive")]
    public void ParseFromDescriptionShouldBeCaseInsensitive(string description)
    {
        var result = EnumExtensions.ParseFromDescription<ETestResultStatus>(description);
        result.Should().Be(ETestResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw when parsing from null description")]
    public void ParseFromDescriptionShouldThrowWhenNull()
    {
        var action = () => EnumExtensions.ParseFromDescription<ETestResultStatus>(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw when description not found in enum")]
    public void ParseFromDescriptionShouldThrowWhenNotFound()
    {
        var action = () => EnumExtensions.ParseFromDescription<ETestResultStatus>("Invalid");
        action.Should().Throw<ArgumentException>().WithMessage("Description 'Invalid' not found in enum ETestResultStatus");
    }

    [Theory]
    [InlineData(" Success ")]
    [InlineData(" HasValidation ")]
    [Trait("Category", "Unit")]
    [DisplayName("Should parse enum from description with whitespace")]
    public void ParseFromDescriptionShouldTrimWhitespace(string description)
    {
        var result = EnumExtensions.ParseFromDescription<EResultStatus>(description);
        result.Should().Be(description.Trim() == "Success" ? EResultStatus.Success : EResultStatus.HasValidation);
    }

    [Theory]
    [InlineData("success", EResultStatus.Success)]
    [InlineData("hasvalidation", EResultStatus.HasValidation)]
    [Trait("Category", "Unit")]
    [DisplayName("Should parse enum with different casing formats")]
    public void ParseFromDescriptionShouldHandleDifferentCasing(string description, EResultStatus expected)
    {
        var result = EnumExtensions.ParseFromDescription<EResultStatus>(description);
        result.Should().Be(expected);
    }
}
