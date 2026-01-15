using BCrypt.Net;

using OnForkHub.Application.Services;
using OnForkHub.Core.Interfaces.Repositories;

using UserEntity = OnForkHub.Core.Entities.User;

namespace OnForkHub.Application.Test.Services;

public class UserServiceTest
{
    private readonly IUserRepositoryEF _userRepository;

    private readonly UserService _userService;

    public UserServiceTest()
    {
        _userRepository = Substitute.For<IUserRepositoryEF>();
        _userService = new UserService(_userRepository);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should register user successfully")]
    public async Task ShouldRegisterUserSuccessfully()
    {
        var name = "John Silva";
        var email = "john@email.com";
        var password = "Password123!";
        var userName = Name.Create(name);
        var user = UserEntity.Create(userName, email, "hashed_password").Data!;

        _userRepository.ExistsByEmailAsync(email).Returns(false);
        _userRepository.CreateAsync(Arg.Any<UserEntity>()).Returns(RequestResult<UserEntity>.Success(user));

        var result = await _userService.RegisterAsync(name, email, password);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        await _userRepository.Received(1).ExistsByEmailAsync(email);
        await _userRepository.Received(1).CreateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when registering with existing email")]
    public async Task ShouldReturnErrorWhenRegisteringWithExistingEmail()
    {
        var name = "John Silva";
        var email = "existing@email.com";
        var password = "Password123!";

        _userRepository.ExistsByEmailAsync(email).Returns(true);

        var result = await _userService.RegisterAsync(name, email, password);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("already exists");
        await _userRepository.Received(1).ExistsByEmailAsync(email);
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when registering with invalid email")]
    public async Task ShouldReturnErrorWhenRegisteringWithInvalidEmail()
    {
        var name = "John Silva";
        var email = "invalid-email";
        var password = "Password123!";

        _userRepository.ExistsByEmailAsync(email).Returns(false);

        var result = await _userService.RegisterAsync(name, email, password);

        result.Status.Should().Be(EResultStatus.HasError);
        await _userRepository.Received(1).ExistsByEmailAsync(email);
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should login successfully with valid credentials")]
    public async Task ShouldLoginSuccessfullyWithValidCredentials()
    {
        var email = "john@email.com";
        var password = "Password123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        var userName = Name.Create("John Silva");
        var user = UserEntity.Create(userName, email, passwordHash).Data!;

        _userRepository.GetByEmailAsync(email).Returns(RequestResult<UserEntity>.Success(user));

        var result = await _userService.LoginAsync(email, password);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(user);
        await _userRepository.Received(1).GetByEmailAsync(email);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when login with non-existent email")]
    public async Task ShouldReturnErrorWhenLoginWithNonExistentEmail()
    {
        var email = "nonexistent@email.com";
        var password = "Password123!";

        _userRepository.GetByEmailAsync(email).Returns(RequestResult<UserEntity>.WithError("User not found"));

        var result = await _userService.LoginAsync(email, password);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Invalid email or password");
        await _userRepository.Received(1).GetByEmailAsync(email);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when login with invalid password")]
    public async Task ShouldReturnErrorWhenLoginWithInvalidPassword()
    {
        var email = "john@email.com";
        var correctPassword = "Password123!";
        var wrongPassword = "WrongPassword";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
        var userName = Name.Create("John Silva");
        var user = UserEntity.Create(userName, email, passwordHash).Data!;

        _userRepository.GetByEmailAsync(email).Returns(RequestResult<UserEntity>.Success(user));

        var result = await _userService.LoginAsync(email, wrongPassword);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Invalid email or password");
        await _userRepository.Received(1).GetByEmailAsync(email);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should get user by id successfully")]
    public async Task ShouldGetUserByIdSuccessfully()
    {
        var userId = Id.Create();
        var userName = Name.Create("John Silva");
        var user = UserEntity.Create(userName, "john@email.com", "hashed_password").Data!;

        _userRepository.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(user));

        var result = await _userService.GetByIdAsync(userId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(user);
        await _userRepository.Received(1).GetByIdAsync(userId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when user not found by id")]
    public async Task ShouldReturnErrorWhenUserNotFoundById()
    {
        var userId = Id.Create();

        _userRepository.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.WithError("Not found"));

        var result = await _userService.GetByIdAsync(userId);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("not found");
        await _userRepository.Received(1).GetByIdAsync(userId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should get user by email successfully")]
    public async Task ShouldGetUserByEmailSuccessfully()
    {
        var email = "john@email.com";
        var userName = Name.Create("John Silva");
        var user = UserEntity.Create(userName, email, "hashed_password").Data!;

        _userRepository.GetByEmailAsync(email).Returns(RequestResult<UserEntity>.Success(user));

        var result = await _userService.GetByEmailAsync(email);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(user);
        await _userRepository.Received(1).GetByEmailAsync(email);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when user not found by email")]
    public async Task ShouldReturnErrorWhenUserNotFoundByEmail()
    {
        var email = "notfound@email.com";

        _userRepository.GetByEmailAsync(email).Returns(RequestResult<UserEntity>.WithError("Not found"));

        var result = await _userService.GetByEmailAsync(email);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("not found");
        await _userRepository.Received(1).GetByEmailAsync(email);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update user successfully")]
    public async Task ShouldUpdateUserSuccessfully()
    {
        var userName = Name.Create("John Silva");
        var user = UserEntity.Create(userName, "john@email.com", "hashed_password").Data!;

        _userRepository.UpdateAsync(user).Returns(RequestResult<UserEntity>.Success(user));

        var result = await _userService.UpdateAsync(user);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(user);
        await _userRepository.Received(1).UpdateAsync(user);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when updating null user")]
    public async Task ShouldReturnErrorWhenUpdatingNullUser()
    {
        UserEntity? user = null;

        var result = await _userService.UpdateAsync(user!);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("cannot be null");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should change password successfully")]
    public async Task ShouldChangePasswordSuccessfully()
    {
        var userId = Id.Create();
        var currentPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(currentPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
        var userName = Name.Create("John Silva");
        var user = UserEntity.Create(userName, "john@email.com", passwordHash).Data!;

        _userRepository.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(user));
        _userRepository.UpdateAsync(user).Returns(RequestResult<UserEntity>.Success(user));

        var result = await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);

        result.Status.Should().Be(EResultStatus.Success);
        await _userRepository.Received(1).GetByIdAsync(userId);
        await _userRepository.Received(1).UpdateAsync(user);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when changing password for non-existent user")]
    public async Task ShouldReturnErrorWhenChangingPasswordForNonExistentUser()
    {
        var userId = Id.Create();
        var currentPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";

        _userRepository.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.WithError("Not found"));

        var result = await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("User not found");
        await _userRepository.Received(1).GetByIdAsync(userId);
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when changing password with incorrect current password")]
    public async Task ShouldReturnErrorWhenChangingPasswordWithIncorrectCurrentPassword()
    {
        var userId = Id.Create();
        var currentPassword = "CorrectPassword123!";
        var wrongCurrentPassword = "WrongPassword";
        var newPassword = "NewPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(currentPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
        var userName = Name.Create("John Silva");
        var user = UserEntity.Create(userName, "john@email.com", passwordHash).Data!;

        _userRepository.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(user));

        var result = await _userService.ChangePasswordAsync(userId, wrongCurrentPassword, newPassword);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Current password is incorrect");
        await _userRepository.Received(1).GetByIdAsync(userId);
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return true when user exists by email")]
    public async Task ShouldReturnTrueWhenUserExistsByEmail()
    {
        var email = "existing@email.com";

        _userRepository.ExistsByEmailAsync(email).Returns(true);

        var result = await _userService.ExistsByEmailAsync(email);

        result.Should().BeTrue();
        await _userRepository.Received(1).ExistsByEmailAsync(email);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return false when user does not exist by email")]
    public async Task ShouldReturnFalseWhenUserDoesNotExistByEmail()
    {
        var email = "nonexistent@email.com";

        _userRepository.ExistsByEmailAsync(email).Returns(false);

        var result = await _userService.ExistsByEmailAsync(email);

        result.Should().BeFalse();
        await _userRepository.Received(1).ExistsByEmailAsync(email);
    }
}
