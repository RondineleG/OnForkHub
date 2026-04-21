namespace OnForkHub.Application.UseCases.Users;

using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.Dtos.User.Response;

/// <summary>
/// Use case for updating a user's profile.
/// </summary>
public class UpdateUserProfileUseCase(IUserService userService) : IUseCase<(Id UserId, UpdateUserProfileRequestDto Request), UserResponseDto>
{
    private readonly IUserService _userService = userService;

    /// <inheritdoc/>
    public async Task<RequestResult<UserResponseDto>> ExecuteAsync((Id UserId, UpdateUserProfileRequestDto Request) input)
    {
        ArgumentNullException.ThrowIfNull(input.UserId);
        ArgumentNullException.ThrowIfNull(input.Request);

        // Get the existing user
        var getUserResult = await _userService.GetByIdAsync(input.UserId);

        if (getUserResult.Status != EResultStatus.Success || getUserResult.Data is null)
        {
            return RequestResult<UserResponseDto>.WithError(getUserResult.Message);
        }

        var user = getUserResult.Data;

        // Update user data
        var name = Name.Create(input.Request.Name);
        var updateResult = user.UpdateData(name, input.Request.Email);

        if (updateResult.Status != EResultStatus.Success)
        {
            return RequestResult<UserResponseDto>.WithError(updateResult.Message);
        }

        // Save the updated user
        var saveResult = await _userService.UpdateAsync(user);

        if (saveResult.Status != EResultStatus.Success || saveResult.Data is null)
        {
            return RequestResult<UserResponseDto>.WithError(saveResult.Message);
        }

        var userResponse = UserResponseDto.FromUser(saveResult.Data);
        return RequestResult<UserResponseDto>.Success(userResponse);
    }
}
