namespace OnForkHub.Application.UseCases.Users;

using OnForkHub.Core.Requests.Users;
using OnForkHub.Core.Responses.Users;

/// <summary>
/// Use case for updating a user's profile.
/// </summary>
public class UpdateUserProfileUseCase(IUserService userService) : IUseCase<(Id UserId, UpdateUserProfileRequest Request), UserProfileResponse>
{
    private readonly IUserService _userService = userService;

    /// <inheritdoc/>
    public async Task<RequestResult<UserProfileResponse>> ExecuteAsync((Id UserId, UpdateUserProfileRequest Request) input)
    {
        ArgumentNullException.ThrowIfNull(input.UserId);
        ArgumentNullException.ThrowIfNull(input.Request);

        // Get the existing user
        var getUserResult = await _userService.GetByIdAsync(input.UserId);

        if (getUserResult.Status != EResultStatus.Success || getUserResult.Data is null)
        {
            return RequestResult<UserProfileResponse>.WithError(getUserResult.Message);
        }

        var user = getUserResult.Data;

        // Update user data
        var name = Name.Create(input.Request.Name);
        var updateResult = user.UpdateData(name, input.Request.Email);

        if (updateResult.Status != EResultStatus.Success)
        {
            return RequestResult<UserProfileResponse>.WithError(updateResult.Message);
        }

        // Save the updated user
        var saveResult = await _userService.UpdateAsync(user);

        if (saveResult.Status != EResultStatus.Success || saveResult.Data is null)
        {
            return RequestResult<UserProfileResponse>.WithError(saveResult.Message);
        }

        var userResponse = UserProfileResponse.FromUser(saveResult.Data);
        return RequestResult<UserProfileResponse>.Success(userResponse);
    }
}
