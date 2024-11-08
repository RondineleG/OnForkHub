namespace OnForkHub.Core.Test.ValueObjects;

public class EmailTests
{
    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("user+test@domain.com")]
    [InlineData("user.name@sub.domain.co.uk")]
    [InlineData("user@example.io")]
    [DisplayName("Should accept valid emails with allowed special characters")]
    public void ShouldAcceptValidEmailWithSpecialCharacters(string emailValue)
    {
        var email = Email.Create(emailValue);
        email.Value.Should().Be(emailValue);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should consider emails equal ignoring case")]
    public void ShouldConsiderEmailsEqualIgnoringCase()
    {
        var email1 = Email.Create("test@DOMAIN.com");
        var email2 = Email.Create("TEST@domain.com");

        email1.Should().Be(email2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create a valid email when the format is correct")]
    public void ShouldCreateValidEmail()
    {
        var email = Email.Create("example@domain.com");
        email.Value.Should().Be("example@domain.com");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("   ")]
    [DisplayName("Should throw exception for empty or whitespace-only email")]
    public void ShouldThrowExceptionForEmptyEmail(string emailValue)
    {
        Action action = () => Email.Create(emailValue);
        action.Should().Throw<DomainException>().WithMessage(EmailResources.EmailCannotBeEmpty);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("invalidemail.com")]
    [InlineData("invalid@")]
    [InlineData("invalid@com.")]
    [InlineData("invalid@com")]
    [InlineData("@domain.com")]
    [DisplayName("Should throw exception for invalid email formats")]
    public void ShouldThrowExceptionForInvalidFormat(string invalidEmail)
    {
        Action action = () => Email.Create(invalidEmail);
        action.Should().Throw<DomainException>().WithMessage(EmailResources.InvalidEmail);
    }
}
