namespace OnForkHub.Application.UseCases.Users;

using OnForkHub.Core.Responses.Users;

/// <summary>
/// Use case for retrieving a user's profile.
/// </summary>
public class GetUserProfileUseCase(IUserService userService) : IUseCase<Id, UserProfileResponse>
{
    private readonly IUserService _userService = userService;

    /// <inheritdoc/>
    public async Task<RequestResult<UserProfileResponse>> ExecuteAsync(Id userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var result = await _userService.GetByIdAsync(userId);

        if (result.Status != EResultStatus.Success || result.Data is null)
        {
            return RequestResult<UserProfileResponse>.WithError(result.Message);
        }

        var userResponse = UserProfileResponse.FromUser(result.Data);
        return RequestResult<UserProfileResponse>.Success(userResponse);
    }
}
