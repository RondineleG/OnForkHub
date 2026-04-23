namespace OnForkHub.Application.UseCases.Users;

using OnForkHub.Application.Dtos.User.Request;

/// <summary>
/// Use case for user registration.
/// </summary>
public class RegisterUserUseCase(IUserService userService) : IUseCase<UserRegisterRequestDto, UserEntity>
{
    private readonly IUserService _userService = userService;

    /// <inheritdoc/>
    public async Task<RequestResult<UserEntity>> ExecuteAsync(UserRegisterRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _userService.RegisterAsync(request.Name, request.Email, request.Password);

        return result.Status != EResultStatus.Success || result.Data is null
            ? RequestResult<UserEntity>.WithError(result.Message)
            : RequestResult<UserEntity>.Success(result.Data);
    }
}
