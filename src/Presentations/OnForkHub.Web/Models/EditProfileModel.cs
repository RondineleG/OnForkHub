namespace OnForkHub.Web.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Model for editing user profile.
/// </summary>
public class EditProfileModel
{
    /// <summary>Gets or sets the user display name.</summary>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the avatar URL.</summary>
    [Url(ErrorMessage = "URL do avatar inválida.")]
    public string? AvatarUrl { get; set; }
}
