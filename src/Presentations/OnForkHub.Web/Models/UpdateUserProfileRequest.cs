namespace OnForkHub.Web.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request DTO for updating user profile.
/// </summary>
public class UpdateUserProfileRequest
{
    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the avatar URL.
    /// </summary>
    [Url(ErrorMessage = "A URL do avatar deve ser válida")]
    public string? AvatarUrl { get; set; }
}
