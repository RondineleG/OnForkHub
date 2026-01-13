namespace OnForkHub.Application.Dtos.User.Request;

/// <summary>
/// Data transfer object for user login requests.
/// </summary>
public sealed class UserLoginRequestDto
{
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
    public string Password { get; set; } = string.Empty;
}
