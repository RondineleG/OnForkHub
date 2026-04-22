namespace OnForkHub.Application.Services;

using System.Diagnostics;

using Microsoft.Extensions.Logging;

using OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service implementation for video transcoding using FFmpeg.
/// </summary>
public sealed partial class VideoTranscodingService(ILogger<VideoTranscodingService> logger) : IVideoTranscodingService
{
    private readonly ILogger<VideoTranscodingService> _logger = logger;

    /// <inheritdoc/>
    public async Task<TranscodingResult> TranscodeToAdaptiveBitrateAsync(
        string inputPath,
        string outputDirectory,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            LogTranscodingStarted(inputPath, outputDirectory);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Define resolutions and bitrates
            var profiles = new[]
            {
                new
                {
                    Resolution = "1920x1080",
                    Bitrate = "5000k",
                    Name = "1080p",
                },
                new
                {
                    Resolution = "1280x720",
                    Bitrate = "2500k",
                    Name = "720p",
                },
                new
                {
                    Resolution = "854x480",
                    Bitrate = "1000k",
                    Name = "480p",
                },
            };

            foreach (var profile in profiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var outputFile = System.IO.Path.Combine(outputDirectory, $"video_{profile.Name}.mp4");
                var args = $"-i \"{inputPath}\" -vf scale={profile.Resolution} -b:v {profile.Bitrate} -c:a copy -y \"{outputFile}\"";

                await RunFFmpegAsync(args, cancellationToken);
            }

            LogTranscodingCompleted(inputPath);

            return new TranscodingResult { Success = true, ManifestPath = System.IO.Path.Combine(outputDirectory, "manifest.mpd") };
        }
        catch (Exception ex)
        {
            LogTranscodingError(ex, inputPath);
            return new TranscodingResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private static async Task RunFFmpegAsync(string args, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = args,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        // FFmpeg writes progress to standard error
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new Exception($"FFmpeg exited with code {process.ExitCode}: {error}");
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting transcoding for {InputPath} to {OutputDirectory}")]
    private partial void LogTranscodingStarted(string inputPath, string outputDirectory);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transcoding completed for {InputPath}")]
    private partial void LogTranscodingCompleted(string inputPath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error during transcoding of {InputPath}")]
    private partial void LogTranscodingError(Exception ex, string inputPath);
}
