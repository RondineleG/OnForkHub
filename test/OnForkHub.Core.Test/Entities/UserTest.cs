namespace OnForkHub.Core.Test.Entities;

public class UserTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create user successfully")]
    public void ShouldCreateUserSuccessfully()
    {
        var user = User.Create("John Silva", "john@email.com");

        user.Should().NotBeNull();
        user.Name.Should().Be("John Silva");
        user.Email.Value.Should().Be("john@email.com");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should keep CreatedAt unchanged after update")]
    public void ShouldKeepCreatedAtUnchangedAfterUpdate()
    {
        var user = User.Create("John Silva", "john@email.com");
        var creationDate = user.CreatedAt;

        user.UpdateName("John Pereira");

        user.CreatedAt.Should().Be(creationDate);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when trying to create user with empty email")]
    public void ShouldThrowExceptionWhenEmailIsEmpty()
    {
        Action act = () => User.Create("John Silva", "");

        act.Should().Throw<DomainException>().WithMessage("Email cannot be empty");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("email@")]
    [InlineData("@domain.com")]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when creating user with invalid email")]
    public void ShouldThrowExceptionWhenEmailIsInvalid(string invalidEmail)
    {
        Action act = () => User.Create("John Silva", invalidEmail);

        act.Should().Throw<DomainException>().WithMessage("Invalid email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Jo")]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when creating user with invalid name")]
    public void ShouldThrowExceptionWhenNameIsInvalid(string invalidName)
    {
        Action act = () => User.Create(invalidName, "email@test.com");

        act.Should().Throw<DomainException>().WithMessage("User name is invalid");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when updating user with invalid email")]
    public void ShouldThrowExceptionWhenUpdatingWithInvalidEmail()
    {
        var user = User.Create("John Silva", "john@email.com");

        Action act = () => user.UpdateEmail("email@");

        act.Should().Throw<DomainException>().WithMessage("Invalid email");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when updating user with invalid name")]
    public void ShouldThrowExceptionWhenUpdatingWithInvalidName()
    {
        var user = User.Create("John Silva", "john@email.com");

        Action act = () => user.UpdateName("Jo");

        act.Should().Throw<DomainException>().WithMessage("User name is invalid");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should successfully update user's name and email")]
    public void ShouldUpdateUserNameAndEmailSuccessfully()
    {
        var user = User.Create("John Silva", "john@email.com");
        user.UpdateName("John Pereira");
        user.UpdateEmail("john.pereira@email.com");

        user.Name.Should().Be("John Pereira");
        user.Email.Value.Should().Be("john.pereira@email.com");
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
