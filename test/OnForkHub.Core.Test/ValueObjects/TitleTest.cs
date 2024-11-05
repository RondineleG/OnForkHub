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

    [Theory]
    [InlineData("", "Title is required")]
    [InlineData("aa", "Title must be at least 3 characters long")]
    [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz", "Title must be no more than 50 characters")]
    [DisplayName("Should return validation error for invalid title")]
    public void ShouldReturnValidationErrorForInvalidTitle(string titleStr, string errorMessage)
    {
        var title = Title.Create(titleStr);
        var validationResult = title.Validate();

        validationResult
            .Errors.Should()
            .ContainSingle(error => error.Message == errorMessage && error.Field == "Title");
    }
}
