using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.Dtos.User.Response;
using OnForkHub.Application.UseCases.Users;

using UserEntity = OnForkHub.Core.Entities.User;

namespace OnForkHub.Application.Test.UseCases.Users;

public class UpdateUserProfileUseCaseTest
{
    private readonly IUserService _userService;
    private readonly UpdateUserProfileUseCase _useCase;

    public UpdateUserProfileUseCaseTest()
    {
        _userService = Substitute.For<IUserService>();
        _useCase = new UpdateUserProfileUseCase(_userService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update user profile successfully")]
    public async Task ShouldUpdateUserProfileSuccessfully()
    {
        // Arrange
        var userId = Id.Create();
        var request = CreateValidUpdateRequest();
        var existingUser = CreateValidUser(userId);

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(existingUser));
        _userService.UpdateAsync(existingUser).Returns(RequestResult<UserEntity>.Success(existingUser));

        // Act
        var result = await _useCase.ExecuteAsync((userId, request));

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(request.Name);
        result.Data.Email.Should().Be(request.Email);
        await _userService.Received(1).GetByIdAsync(userId);
        await _userService.Received(1).UpdateAsync(existingUser);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when userId is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenUserIdIsNull()
    {
        // Arrange
        Id? userId = null;
        var request = CreateValidUpdateRequest();

        // Act
        var act = () => _useCase.ExecuteAsync((userId!, request));

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
        await _userService.DidNotReceive().GetByIdAsync(Arg.Any<Id>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when request is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        // Arrange
        var userId = Id.Create();

        // Act
        var act = () => _useCase.ExecuteAsync((userId, null!));

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
        var request = CreateValidUpdateRequest();

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.WithError("User not found"));

        // Act
        var result = await _useCase.ExecuteAsync((userId, request));

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("User not found");
        await _userService.Received(1).GetByIdAsync(userId);
        await _userService.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when service returns null data on get")]
    public async Task ShouldReturnErrorWhenServiceReturnsNullDataOnGet()
    {
        // Arrange
        var userId = Id.Create();
        var request = CreateValidUpdateRequest();

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync((userId, request));

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        await _userService.DidNotReceive().UpdateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when update operation fails")]
    public async Task ShouldReturnErrorWhenUpdateOperationFails()
    {
        // Arrange
        var userId = Id.Create();
        var request = CreateValidUpdateRequest();
        var existingUser = CreateValidUser(userId);

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(existingUser));
        _userService.UpdateAsync(existingUser).Returns(RequestResult<UserEntity>.WithError("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync((userId, request));

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Database error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when save operation fails")]
    public async Task ShouldReturnErrorWhenSaveOperationFails()
    {
        // Arrange
        var userId = Id.Create();
        var request = CreateValidUpdateRequest();
        var existingUser = CreateValidUser(userId);

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(existingUser));
        _userService.UpdateAsync(existingUser).Returns(RequestResult<UserEntity>.WithError("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync((userId, request));

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Database error");
        await _userService.Received(1).UpdateAsync(existingUser);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when save returns null data")]
    public async Task ShouldReturnErrorWhenSaveReturnsNullData()
    {
        // Arrange
        var userId = Id.Create();
        var request = CreateValidUpdateRequest();
        var existingUser = CreateValidUser(userId);

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(existingUser));
        _userService.UpdateAsync(existingUser).Returns(RequestResult<UserEntity>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync((userId, request));

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        await _userService.Received(1).UpdateAsync(existingUser);
    }

    private static UpdateUserProfileRequest CreateValidUpdateRequest()
    {
        return new UpdateUserProfileRequest { Name = "Updated Name", Email = "updated@email.com" };
    }

    private static UserEntity CreateValidUser(Id id)
    {
        var name = Name.Create("Original Name");
        var user = UserEntity.Load(id, name, "original@email.com", "hashed_password", DateTime.UtcNow).Data!;
        return user;
    }
}
