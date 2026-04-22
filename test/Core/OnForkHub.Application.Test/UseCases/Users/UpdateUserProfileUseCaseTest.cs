using OnForkHub.Application.UseCases.Users;
using OnForkHub.Core.Requests.Users;
using OnForkHub.Core.Responses.Users;

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
        var user = CreateValidUser(userId);
        var request = CreateValidUpdateRequest();

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(user));
        _userService.UpdateAsync(Arg.Any<UserEntity>()).Returns(args => RequestResult<UserEntity>.Success((UserEntity)args[0]));

        // Act
        var result = await _useCase.ExecuteAsync((userId, request));

        // Assert
        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(request.Name);
        result.Data.Email.Should().Be(request.Email);
        await _userService.Received(1).GetByIdAsync(userId);
        await _userService.Received(1).UpdateAsync(Arg.Is<UserEntity>(u => u.Id == userId));
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
    [DisplayName("Should return error when update data fails (validation)")]
    public async Task ShouldReturnErrorWhenUpdateDataFails()
    {
        // Arrange
        var userId = Id.Create();
        var user = CreateValidUser(userId);
        var request = new UpdateUserProfileRequest { Name = string.Empty, Email = "invalid-email" }; // Invalid data

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(user));

        // Act & Assert
        var act = () => _useCase.ExecuteAsync((userId, request));
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when save fails")]
    public async Task ShouldReturnErrorWhenSaveFails()
    {
        // Arrange
        var userId = Id.Create();
        var user = CreateValidUser(userId);
        var request = CreateValidUpdateRequest();

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(user));
        _userService.UpdateAsync(Arg.Any<UserEntity>()).Returns(RequestResult<UserEntity>.WithError("Database error"));

        // Act
        var result = await _useCase.ExecuteAsync((userId, request));

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        result.Message.Should().Be("Database error");
        await _userService.Received(1).GetByIdAsync(userId);
        await _userService.Received(1).UpdateAsync(Arg.Any<UserEntity>());
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
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when request is null")]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        // Arrange
        var userId = Id.Create();
        UpdateUserProfileRequest? request = null;

        // Act
        var act = () => _useCase.ExecuteAsync((userId, request!));

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when service returns null data on save")]
    public async Task ShouldReturnErrorWhenServiceReturnsNullDataOnSave()
    {
        // Arrange
        var userId = Id.Create();
        var user = CreateValidUser(userId);
        var request = CreateValidUpdateRequest();

        _userService.GetByIdAsync(userId).Returns(RequestResult<UserEntity>.Success(user));
        _userService.UpdateAsync(Arg.Any<UserEntity>()).Returns(RequestResult<UserEntity>.Success(null!));

        // Act
        var result = await _useCase.ExecuteAsync((userId, request));

        // Assert
        result.Status.Should().Be(EResultStatus.HasError);
        await _userService.Received(1).GetByIdAsync(userId);
        await _userService.Received(1).UpdateAsync(Arg.Any<UserEntity>());
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
