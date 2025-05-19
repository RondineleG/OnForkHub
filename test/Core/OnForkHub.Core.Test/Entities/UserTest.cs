// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Test.Entities;

public class UserTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create user successfully")]
    public void ShouldCreateUserSuccessfully()
    {
        var name = Name.Create("John Silva");
        var result = User.Create(name, "john@email.com");

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(name);
        result.Data.Email.Value.Should().Be("john@email.com");
        result.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Data.UpdatedAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when trying to create user with empty email")]
    public void ShouldThrowExceptionWhenEmailIsEmpty()
    {
        var name = Name.Create("John Silva");

        var result = User.Create(name, string.Empty);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError!.Description.Should().Be(EmailResources.EmailCannotBeEmpty);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("email@")]
    [InlineData("@domain.com")]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when creating user with invalid email")]
    public void ShouldThrowExceptionWhenEmailIsInvalid(string invalidEmail)
    {
        var name = Name.Create("John Silva");

        var result = User.Create(name, invalidEmail);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError!.Description.Should().Be(EmailResources.InvalidEmail);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should successfully update user's name and email")]
    public void ShouldUpdateUserNameAndEmailSuccessfully()
    {
        var name = Name.Create("John Silva");
        var user = User.Create(name, "john@email.com").Data!;

        var updatedName = Name.Create("John Pereira");
        var email = "john.pereira@email.com";

        var result = user.UpdateData(updatedName, email);

        result.Status.Should().Be(EResultStatus.Success);
        user.Name.Should().Be(updatedName);
        user.Email.Value.Should().Be(email);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
