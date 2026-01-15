namespace OnForkHub.Application.Services;

using BCrypt.Net;

/// <summary>
/// Service for User operations including authentication.
/// </summary>
public class UserService(IUserRepositoryEF userRepository) : BaseService, IUserService
{
    private readonly IUserRepositoryEF _userRepository = userRepository;

    /// <inheritdoc/>
    public Task<RequestResult<UserEntity>> RegisterAsync(string name, string email, string password)
    {
        return ExecuteAsync(async () =>
        {
            // Check if user already exists
            if (await _userRepository.ExistsByEmailAsync(email))
            {
                return RequestResult<UserEntity>.WithError($"User with email {email} already exists");
            }

            // Hash the password
            var passwordHash = BCrypt.HashPassword(password, BCrypt.GenerateSalt(12));

            // Create user entity
            var nameResult = Name.Create(name);
            var userResult = User.Create(nameResult, email, passwordHash);

            if (userResult.Status != EResultStatus.Success || userResult.Data is null)
            {
                return RequestResult<UserEntity>.WithError(userResult.Message);
            }

            // Save to database
            return await _userRepository.CreateAsync(userResult.Data);
        });
    }

    /// <inheritdoc/>
    public Task<RequestResult<UserEntity>> LoginAsync(string email, string password)
    {
        return ExecuteAsync(async () =>
        {
            // Get user by email
            var userResult = await _userRepository.GetByEmailAsync(email);

            if (userResult.Status != EResultStatus.Success || userResult.Data is null)
            {
                return RequestResult<UserEntity>.WithError("Invalid email or password");
            }

            var user = userResult.Data;

            // Verify password
            if (!BCrypt.Verify(password, user.PasswordHash))
            {
                return RequestResult<UserEntity>.WithError("Invalid email or password");
            }

            return RequestResult<UserEntity>.Success(user);
        });
    }

    /// <inheritdoc/>
    public Task<RequestResult<UserEntity>> GetByIdAsync(Id id)
    {
        return ExecuteAsync(async () =>
        {
            var result = await _userRepository.GetByIdAsync(id);
            return !result.Status.Equals(EResultStatus.Success) ? RequestResult<UserEntity>.WithError($"User with id {id} not found") : result;
        });
    }

    /// <inheritdoc/>
    public Task<RequestResult<UserEntity>> GetByEmailAsync(string email)
    {
        return ExecuteAsync(async () =>
        {
            var result = await _userRepository.GetByEmailAsync(email);
            return !result.Status.Equals(EResultStatus.Success) ? RequestResult<UserEntity>.WithError($"User with email {email} not found") : result;
        });
    }

    /// <inheritdoc/>
    public Task<RequestResult<UserEntity>> UpdateAsync(UserEntity user)
    {
        return ExecuteAsync(async () =>
        {
            if (user is null)
            {
                return RequestResult<UserEntity>.WithError("User cannot be null");
            }

            return await _userRepository.UpdateAsync(user);
        });
    }

    /// <inheritdoc/>
    public Task<RequestResult> ChangePasswordAsync(Id userId, string currentPassword, string newPassword)
    {
        return ExecuteAsync(async () =>
        {
            // Get user
            var userResult = await _userRepository.GetByIdAsync(userId);
            if (userResult.Status != EResultStatus.Success || userResult.Data is null)
            {
                return RequestResult.WithError("User not found");
            }

            var user = userResult.Data;

            // Verify current password
            if (!BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return RequestResult.WithError("Current password is incorrect");
            }

            // Hash new password
            var newPasswordHash = BCrypt.HashPassword(newPassword, BCrypt.GenerateSalt(12));

            // Update password
            var updateResult = user.UpdatePassword(newPasswordHash);
            if (updateResult.Status != EResultStatus.Success)
            {
                return updateResult;
            }

            // Save to database
            var saveResult = await _userRepository.UpdateAsync(user);
            return saveResult.Status == EResultStatus.Success ? RequestResult.Success() : RequestResult.WithError(saveResult.Message);
        });
    }

    /// <inheritdoc/>
    public Task<bool> ExistsByEmailAsync(string email)
    {
        return _userRepository.ExistsByEmailAsync(email);
    }
}
