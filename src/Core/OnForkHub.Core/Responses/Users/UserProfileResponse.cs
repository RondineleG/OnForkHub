namespace OnForkHub.Core.Responses.Users;

using OnForkHub.Core.Entities;
using OnForkHub.Core.ValueObjects;

/// <summary>
/// Response containing user profile information.
/// </summary>
public sealed class UserProfileResponse
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
    /// Gets or sets the user's avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the user's roles.
    /// </summary>
    public IReadOnlyList<string> Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets the user's preferences.
    /// </summary>
    public UserPreferences? Preferences { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Creates a UserProfileResponse from a User entity.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The user profile response.</returns>
    public static UserProfileResponse FromUser(User user, IReadOnlyList<string>? roles = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserProfileResponse
        {
            Id = user.Id.ToString(),
            Name = user.Name.Value,
            Email = user.Email.Value,
            AvatarUrl = user.AvatarUrl,
            Roles = roles ?? [],
            Preferences = user.Preferences,
            CreatedAt = user.CreatedAt,
        };
    }
}
