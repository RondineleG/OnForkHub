namespace OnForkHub.Application.Dtos.Video.Request;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for initiating a chunked video upload.
/// </summary>
public sealed class InitiateUploadRequest
{
    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    [Required(ErrorMessage = "The FileName field is required")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total file size in bytes.
    /// </summary>
    [Required(ErrorMessage = "The FileSize field is required")]
    [Range(1, long.MaxValue, ErrorMessage = "The FileSize must be greater than 0")]
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the content type (MIME type).
    /// </summary>
    [Required(ErrorMessage = "The ContentType field is required")]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of chunks.
    /// </summary>
    [Required(ErrorMessage = "The TotalChunks field is required")]
    [Range(1, int.MaxValue, ErrorMessage = "The TotalChunks must be at least 1")]
    public int TotalChunks { get; set; }

    /// <summary>
    /// Gets or sets the user ID who owns the upload.
    /// </summary>
    [Required(ErrorMessage = "The UserId field is required")]
    public string UserId { get; set; } = string.Empty;
}
