namespace OnForkHub.Core.Responses;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;

/// <summary>
/// Response for video upload status and information.
/// </summary>
public sealed class VideoUploadResponse
{
    /// <summary>
    /// Gets or sets the upload identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the upload status.
    /// </summary>
    public EVideoUploadStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage.
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Gets or sets the storage path if completed.
    /// </summary>
    public string? StoragePath { get; set; }

    /// <summary>
    /// Creates a VideoUploadResponse from a VideoUpload entity.
    /// </summary>
    /// <param name="upload">The video upload entity.</param>
    /// <returns>The video upload response.</returns>
    public static VideoUploadResponse FromEntity(VideoUpload upload)
    {
        ArgumentNullException.ThrowIfNull(upload);

        return new VideoUploadResponse
        {
            Id = upload.Id.ToString(),
            FileName = upload.FileName,
            Status = upload.Status,
            ProgressPercentage = upload.ProgressPercentage,
            StoragePath = upload.StoragePath,
        };
    }
}
