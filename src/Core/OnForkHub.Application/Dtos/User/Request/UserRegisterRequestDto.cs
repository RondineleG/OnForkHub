namespace OnForkHub.Application.Dtos.User.Request;

using OnForkHub.Core.Entities;
using OnForkHub.Core.ValueObjects;

/// <summary>
/// Data transfer object for user registration requests.
/// </summary>
public sealed class UserRegisterRequestDto
{
    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    [Required(ErrorMessage = "The Name field is required")]
    [MaxLength(100, ErrorMessage = "The Name field must be at most 100 characters long")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email.
    /// </summary>
    [Required(ErrorMessage = "The Email field is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    [Required(ErrorMessage = "The Password field is required")]
    [MinLength(8, ErrorMessage = "The Password field must be at least 8 characters long")]
    public string Password { get; set; } = string.Empty;
}
