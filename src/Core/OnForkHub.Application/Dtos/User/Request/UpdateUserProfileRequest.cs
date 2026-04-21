namespace OnForkHub.Application.Dtos.User.Request;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request for updating a user's profile.
/// </summary>
public sealed class UpdateUserProfileRequest
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
}
