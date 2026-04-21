using OnForkHub.Application.Dtos.User.Response;
using OnForkHub.Application.UseCases.Users;
using UserEntity = OnForkHub.Core.Entities.User;

namespace OnForkHub.Application.Test.UseCases.Users;

public class GetUserProfileUseCaseTest
{
    private readonly IUserService _userService;
    private readonly GetUserProfileUseCase _useCase;

    public GetUserProfileUseCaseTest()
    {
        _userService = Substitute.For<IUserService>();
        _useCase = new GetUserProfileUseCase(_userService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should get user profile successfully")]
    public async Task ShouldGetUserProfileSuccessfully()
    {
        // Arrange
        var userId = Id.Create();
        var user = CreateValidUser(userId);

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(user));

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Contain(userId.ToString());
        result.Data.Name.Should().Be(user.Name.Value);
        result.Data.Email.Should().Be(user.Email.Value);
        await _userService.Received(1).GetByIdAsync(userId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when userId is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenUserIdIsNull()
    {
        // Arrange
        Id? userId = null;

        // Act
        var act = () => _useCase.ExecuteAsync(userId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        await _userService.DidNotReceive().GetByIdAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when user not found")]
    public async Task ShouldReturnErrorWhenUserNotFound()
    {
        // Arrange
        var userId = Id.Create();

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.WithError("User not found"));

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("User not found");
        await _userService.Received(1).GetByIdAsync(userId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when service returns null data")]
    public async Task ShouldReturnErrorWhenServiceReturnsNullData()
    {
        // Arrange
        var userId = Id.Create();

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        await _userService.Received(1).GetByIdAsync(userId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error with service message when operation fails")]
    public async Task ShouldReturnErrorWithServiceMessageWhenOperationFails()
    {
        // Arrange
        var userId = Id.Create();

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.WithError("Database connection error"));

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Database connection error");
    }

    private static UserEntity CreateValidUser(Id id)
    {
        var name = Name.Create("John Silva");
        var user = UserEntity.Load(id, name, "john@email.com", "hashed_password", DateTime.UtcNow).Data!;
        return user;
    }
}
