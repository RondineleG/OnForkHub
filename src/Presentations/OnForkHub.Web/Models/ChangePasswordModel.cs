namespace OnForkHub.Web.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Model for changing password.
/// </summary>
public class ChangePasswordModel
{
    /// <summary>Gets or sets the current password.</summary>
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>Gets or sets the new password.</summary>
    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>Gets or sets the confirmation of the new password.</summary>
    [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
    [Compare("NewPassword", ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
