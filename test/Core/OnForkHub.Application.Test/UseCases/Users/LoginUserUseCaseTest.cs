using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.UseCases.Users;

using UserEntity = OnForkHub.Core.Entities.User;

namespace OnForkHub.Application.Test.UseCases.Users;

public class LoginUserUseCaseTest
{
    private readonly IUserService _userService;

    private readonly LoginUserUseCase _loginUserUseCase;

    public LoginUserUseCaseTest()
    {
        _userService = Substitute.For<IUserService>();
        _loginUserUseCase = new LoginUserUseCase(_userService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should login user successfully")]
    public async Task ShouldLoginUserSuccessfully()
    {
        var request = new UserLoginRequestDto { Email = "john@email.com", Password = "Password123!" };

        var userName = Name.Create("John Silva");
        var user = UserEntity.Create(userName, request.Email, "hashed_password").Data!;

        _userService.LoginAsync(request.Email, request.Password).Returns(RequestResult<UserEntity>.Success(user));

        var result = await _loginUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(user);
        await _userService.Received(1).LoginAsync(request.Email, request.Password);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when credentials are invalid")]
    public async Task ShouldReturnErrorWhenCredentialsAreInvalid()
    {
        var request = new UserLoginRequestDto { Email = "john@email.com", Password = "WrongPassword" };

        _userService.LoginAsync(request.Email, request.Password).Returns(RequestResult<UserEntity>.WithError("Invalid email or password"));

        var result = await _loginUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
        await _userService.Received(1).LoginAsync(request.Email, request.Password);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when request is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        UserLoginRequestDto? request = null;

        var act = () => _loginUserUseCase.ExecuteAsync(request!);

        await act.Should().ThrowAsync<ArgumentNullException>();
        await _userService.DidNotReceive().LoginAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when service returns null user")]
    public async Task ShouldReturnErrorWhenServiceReturnsNullUser()
    {
        var request = new UserLoginRequestDto { Email = "john@email.com", Password = "Password123!" };

        _userService.LoginAsync(request.Email, request.Password).Returns(RequestResult<UserEntity>.Success(null!));

        var result = await _loginUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
        await _userService.Received(1).LoginAsync(request.Email, request.Password);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when user is not found")]
    public async Task ShouldReturnErrorWhenUserIsNotFound()
    {
        var request = new UserLoginRequestDto { Email = "nonexistent@email.com", Password = "Password123!" };

        _userService.LoginAsync(request.Email, request.Password).Returns(RequestResult<UserEntity>.WithError("User not found"));

        var result = await _loginUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
        await _userService.Received(1).LoginAsync(request.Email, request.Password);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when email is empty")]
    public async Task ShouldReturnErrorWhenEmailIsEmpty()
    {
        var request = new UserLoginRequestDto { Email = string.Empty, Password = "Password123!" };

        _userService.LoginAsync(request.Email, request.Password).Returns(RequestResult<UserEntity>.WithError("Email is required"));

        var result = await _loginUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Email is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when password is empty")]
    public async Task ShouldReturnErrorWhenPasswordIsEmpty()
    {
        var request = new UserLoginRequestDto { Email = "john@email.com", Password = string.Empty };

        _userService.LoginAsync(request.Email, request.Password).Returns(RequestResult<UserEntity>.WithError("Password is required"));

        var result = await _loginUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Password is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error with service message on failure")]
    public async Task ShouldReturnErrorWithServiceMessageOnFailure()
    {
        var request = new UserLoginRequestDto { Email = "john@email.com", Password = "Password123!" };

        _userService.LoginAsync(request.Email, request.Password).Returns(RequestResult<UserEntity>.WithError("Database connection timeout"));

        var result = await _loginUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Database connection timeout");
        await _userService.Received(1).LoginAsync(request.Email, request.Password);
    }
}
