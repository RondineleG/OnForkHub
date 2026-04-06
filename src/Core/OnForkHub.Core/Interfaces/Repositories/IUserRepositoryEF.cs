namespace OnForkHub.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for User entity using Entity Framework.
/// </summary>
public interface IUserRepositoryEF
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <returns>The created user.</returns>
    Task<RequestResult<UserEntity>> CreateAsync(UserEntity user);

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The deleted user.</returns>
    Task<RequestResult<UserEntity>> DeleteAsync(Id id);

    /// <summary>
    /// Gets all users with pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="size">The page size.</param>
    /// <returns>A list of users.</returns>
    Task<RequestResult<IEnumerable<UserEntity>>> GetAllAsync(int page, int size);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user.</returns>
    Task<RequestResult<UserEntity>> GetByIdAsync(Id id);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The user email.</param>
    /// <returns>The user.</returns>
    Task<RequestResult<UserEntity>> GetByEmailAsync(string email);

    /// <summary>
    /// Updates a user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>The updated user.</returns>
    Task<RequestResult<UserEntity>> UpdateAsync(UserEntity user);

    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if the user exists, false otherwise.</returns>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Gets the total count of users.
    /// </summary>
    /// <returns>The total count.</returns>
    Task<int> GetTotalCountAsync();
}
