namespace OnForkHub.Core.Test.ValueObjects;

public class TitleTest
{
    [Theory]
    [InlineData("test")]
    [InlineData("hi!")]
    [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvw")]
    [DisplayName("Create Valid Title")]
    public void CreateValidTitle(string titleStr)
    {
        var title = Title.Create(titleStr);
        var validationResult = title.Validate();

        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    [DisplayName("Should return validation error for long title")]
    public void ShouldReturnValidationErrorForLongTitle()
    {
        var title = Title.Create(new string('a', 51));
        var validationResult = title.Validate();

        validationResult.Errors.Should().ContainSingle(error => error.Message == TitleResources.TitleMaxLength && error.Field == "Title");
    }

    [Fact]
    [DisplayName("Should return validation error for required title")]
    public void ShouldReturnValidationErrorForNullTitle()
    {
        var title = Title.Create(string.Empty);
        var validationResult = title.Validate();

        validationResult.Errors.Should().ContainSingle(error => error.Message == TitleResources.TitleRequired && error.Field == "Title");
    }

    [Fact]
    [DisplayName("Should return validation error for short title")]
    public void ShouldReturnValidationErrorForShortTitle()
    {
        var title = Title.Create("hi");
        var validationResult = title.Validate();

        validationResult.Errors.Should().ContainSingle(error => error.Message == TitleResources.TitleMinLength && error.Field == "Title");
    }
}