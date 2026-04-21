namespace OnForkHub.Web.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

/// <summary>
/// Model for video upload form.
/// </summary>
public class VideoUploadModel
{
    /// <summary>Gets or sets the video title.</summary>
    [Required(ErrorMessage = "O título é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the video description.</summary>
    [MaxLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the category ID.</summary>
    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public long CategoryId { get; set; }

    /// <summary>Gets or sets the tags for the video.</summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>Gets or sets the video file to upload.</summary>
    public IBrowserFile? VideoFile { get; set; }
}
