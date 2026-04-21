namespace OnForkHub.Application.UseCases.Users;

using OnForkHub.Application.Dtos.User.Request;

/// <summary>
/// Use case for user login/authentication.
/// </summary>
public class LoginUserUseCase(IUserService userService) : IUseCase<UserLoginRequestDto, UserEntity>
{
    private readonly IUserService _userService = userService;

    /// <inheritdoc/>
    public async Task<RequestResult<UserEntity>> ExecuteAsync(UserLoginRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _userService.LoginAsync(request.Email, request.Password);

        return result.Status != EResultStatus.Success || result.Data is null
            ? RequestResult<UserEntity>.WithError(result.Message)
            : RequestResult<UserEntity>.Success(result.Data);
    }
}
