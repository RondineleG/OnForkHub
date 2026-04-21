namespace OnForkHub.Application.UseCases.Users;

using OnForkHub.Application.Dtos.User.Response;

/// <summary>
/// Use case for retrieving a user's profile.
/// </summary>
public class GetUserProfileUseCase(IUserService userService) : IUseCase<Id, UserResponseDto>
{
    private readonly IUserService _userService = userService;

    /// <inheritdoc/>
    public async Task<RequestResult<UserResponseDto>> ExecuteAsync(Id userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var result = await _userService.GetByIdAsync(userId);

        if (result.Status != EResultStatus.Success || result.Data is null)
        {
            return RequestResult<UserResponseDto>.WithError(result.Message);
        }

        var userResponse = UserResponseDto.FromUser(result.Data);
        return RequestResult<UserResponseDto>.Success(userResponse);
    }
}
