namespace OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service for transcoding videos using FFmpeg.
/// </summary>
public interface IVideoTranscodingService
{
    /// <summary>
    /// Transcodes a video file to multiple resolutions for adaptive bitrate streaming.
    /// </summary>
    /// <param name="inputPath">Path to the input video file.</param>
    /// <param name="outputDirectory">Directory where transcoded files will be stored.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Transcoding result.</returns>
    Task<TranscodingResult> TranscodeToAdaptiveBitrateAsync(string inputPath, string outputDirectory, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a transcoding operation.
/// </summary>
public class TranscodingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ManifestPath { get; set; }
    public string? ThumbnailPath { get; set; }
}
