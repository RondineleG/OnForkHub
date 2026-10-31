namespace OnForkHub.Core.Enums;

/// <summary>
/// Represents the status of a video upload process.
/// </summary>
public enum EVideoUploadStatus
{
    /// <summary>
    /// Upload is pending and waiting to start.
    /// </summary>
    Pending,

    /// <summary>
    /// Upload is currently in progress.
    /// </summary>
    Uploading,

    /// <summary>
    /// Video is being processed (thumbnail generation, metadata extraction).
    /// </summary>
    Processing,

    /// <summary>
    /// Upload and processing completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Upload or processing failed.
    /// </summary>
    Failed,
}
