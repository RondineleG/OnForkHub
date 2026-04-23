namespace OnForkHub.Web.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Model for creating/updating a category.
/// </summary>
public class CategoryCreateModel
{
    /// <summary>Gets or sets the category name.</summary>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category description.</summary>
    [MaxLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    public string Description { get; set; } = string.Empty;
}
