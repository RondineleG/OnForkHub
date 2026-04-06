namespace OnForkHub.Application.Dtos.User.Response;

/// <summary>
/// Data transfer object for user responses.
/// </summary>
public sealed class UserResponseDto
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's roles.
    /// </summary>
    public IReadOnlyList<string> Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Creates a UserResponseDto from a User entity.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The user response DTO.</returns>
    public static UserResponseDto FromUser(Core.Entities.User user, IReadOnlyList<string>? roles = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserResponseDto
        {
            Id = user.Id.ToString(),
            Name = user.Name.Value,
            Email = user.Email.Value,
            Roles = roles ?? [],
            CreatedAt = user.CreatedAt,
        };
    }
}
