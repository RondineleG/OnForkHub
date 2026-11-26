namespace OnForkHub.Core.Interfaces.Services;

public interface IVideoTranscodingService
{
    Task<TranscodingResult> TranscodeToAdaptiveBitrateAsync(string inputPath, string outputDirectory, CancellationToken cancellationToken = default);
}

public class TranscodingResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>
    /// True when transcoding was skipped because ffmpeg is not available on the host.
    /// The original file remains the streamable source in that case.
    /// </summary>
    public bool Skipped { get; set; }

    /// <summary>
    /// Filesystem path of the generated thumbnail, when ffmpeg is available.
    /// </summary>
    public string? ThumbnailPath { get; set; }

    /// <summary>
    /// Filesystem paths of the generated progressive MP4 variants keyed by profile name (e.g. 720p, 480p).
    /// </summary>
    public IReadOnlyDictionary<string, string> Variants { get; set; } = new Dictionary<string, string>();
}
