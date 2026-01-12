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

    /// <summary>
    /// Creates a User entity from this DTO.
    /// </summary>
    /// <returns>A result containing the created user or errors.</returns>
    public RequestResult<Core.Entities.User> ToUser()
    {
        try
        {
            var name = Core.ValueObjects.Name.Create(Name);
            return Core.Entities.User.Create(name, Email);
        }
        catch (DomainException ex)
        {
            return RequestResult<Core.Entities.User>.WithError(ex.Message);
        }
    }
}
