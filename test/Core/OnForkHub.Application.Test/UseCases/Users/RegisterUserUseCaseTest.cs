using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.UseCases.Users;

using UserEntity = OnForkHub.Core.Entities.User;

namespace OnForkHub.Application.Test.UseCases.Users;

public class RegisterUserUseCaseTest
{
    private readonly IUserService _userService;

    private readonly RegisterUserUseCase _registerUserUseCase;

    public RegisterUserUseCaseTest()
    {
        _userService = Substitute.For<IUserService>();
        _registerUserUseCase = new RegisterUserUseCase(_userService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should register user successfully")]
    public async Task ShouldRegisterUserSuccessfully()
    {
        var request = new UserRegisterRequestDto
        {
            Name = "John Silva",
            Email = "john@email.com",
            Password = "Password123!"
        };

        var userName = Name.Create(request.Name);
        var user = UserEntity.Create(userName, request.Email, "hashed_password").Data!;

        _userService.RegisterAsync(request.Name, request.Email, request.Password)
            .Returns(RequestResult<UserEntity>.Success(user));

        var result = await _registerUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(user);
        await _userService.Received(1).RegisterAsync(request.Name, request.Email, request.Password);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when service returns error")]
    public async Task ShouldReturnErrorWhenServiceReturnsError()
    {
        var request = new UserRegisterRequestDto
        {
            Name = "John Silva",
            Email = "existing@email.com",
            Password = "Password123!"
        };

        _userService.RegisterAsync(request.Name, request.Email, request.Password)
            .Returns(RequestResult<UserEntity>.WithError("User already exists"));

        var result = await _registerUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
        await _userService.Received(1).RegisterAsync(request.Name, request.Email, request.Password);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when request is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        UserRegisterRequestDto? request = null;

        var act = () => _registerUserUseCase.ExecuteAsync(request!);

        await act.Should().ThrowAsync<ArgumentNullException>();
        await _userService.DidNotReceive().RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when service returns null user")]
    public async Task ShouldReturnErrorWhenServiceReturnsNullUser()
    {
        var request = new UserRegisterRequestDto
        {
            Name = "John Silva",
            Email = "john@email.com",
            Password = "Password123!"
        };

        _userService.RegisterAsync(request.Name, request.Email, request.Password)
            .Returns(RequestResult<UserEntity>.Success(null!));

        var result = await _registerUserUseCase.ExecuteAsync(request);

        result.Status.Should().Be(EResultStatus.HasError);
        await _userService.Received(1).RegisterAsync(request.Name, request.Email, request.Password);
    }
}
