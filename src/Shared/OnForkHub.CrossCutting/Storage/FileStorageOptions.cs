namespace OnForkHub.CrossCutting.Storage;

/// <summary>
/// Configuration options for file storage.
/// </summary>
public sealed class FileStorageOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Gets or sets the base path for file storage.
    /// </summary>
    public string BasePath { get; set; } = "uploads";

    /// <summary>
    /// Gets or sets the maximum file size in bytes (default: 500MB).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 500 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the allowed video extensions.
    /// </summary>
    public IReadOnlyList<string> AllowedVideoExtensions { get; set; } = [".mp4", ".avi", ".mov", ".wmv", ".mkv", ".webm"];

    /// <summary>
    /// Gets or sets the allowed video content types.
    /// </summary>
    public IReadOnlyList<string> AllowedVideoContentTypes { get; set; } =
    ["video/mp4", "video/x-msvideo", "video/quicktime", "video/x-ms-wmv", "video/x-matroska", "video/webm"];

    /// <summary>
    /// Gets or sets the base URL for accessing files.
    /// </summary>
    public string BaseUrl { get; set; } = "/files";
}
