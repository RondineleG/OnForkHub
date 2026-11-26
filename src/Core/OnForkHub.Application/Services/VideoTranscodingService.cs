namespace OnForkHub.Application.Services;

using System.Diagnostics;

using Microsoft.Extensions.Logging;

using OnForkHub.Core.Interfaces.Services;

public sealed partial class VideoTranscodingService(ILogger<VideoTranscodingService> logger) : IVideoTranscodingService
{
    private static readonly object ProbeLock = new();

    private static bool? _ffmpegAvailable;

    private readonly ILogger<VideoTranscodingService> _logger = logger;

    public async Task<TranscodingResult> TranscodeToAdaptiveBitrateAsync(
        string inputPath,
        string outputDirectory,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (!IsFFmpegAvailable())
            {
                LogFFmpegUnavailable(inputPath);
                return new TranscodingResult { Success = true, Skipped = true };
            }

            LogTranscodingStarted(inputPath, outputDirectory);
            Directory.CreateDirectory(outputDirectory);

            var thumbnailPath = Path.Combine(outputDirectory, "thumbnail.jpg");
            await RunFFmpegAsync($"-ss 1 -i \"{inputPath}\" -frames:v 1 -q:v 2 -y \"{thumbnailPath}\"", cancellationToken);
            if (!File.Exists(thumbnailPath))
            {
                thumbnailPath = null;
            }

            // Profiles never upscale: the input resolution caps which variants are produced.
            var inputHeight = await ProbeHeightAsync(inputPath, cancellationToken);
            var candidates = new[]
            {
                (Name: "720p", Height: 720, Bitrate: "2500k"),
                (Name: "480p", Height: 480, Bitrate: "1000k"),
                (Name: "360p", Height: 360, Bitrate: "600k"),
            };

            var variants = new Dictionary<string, string>();
            foreach (var profile in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (inputHeight.HasValue && inputHeight.Value <= profile.Height)
                {
                    continue;
                }

                var outputFile = Path.Combine(outputDirectory, $"video_{profile.Name}.mp4");
                var args =
                    $"-i \"{inputPath}\" -vf \"scale=-2:{profile.Height}\" -b:v {profile.Bitrate} "
                    + $"-c:v libx264 -preset fast -c:a aac -movflags +faststart -y \"{outputFile}\"";
                await RunFFmpegAsync(args, cancellationToken);

                if (File.Exists(outputFile))
                {
                    variants[profile.Name] = outputFile;
                }
            }

            LogTranscodingCompleted(inputPath);
            return new TranscodingResult { Success = true, ThumbnailPath = thumbnailPath, Variants = variants };
        }
        catch (Exception ex)
        {
            LogTranscodingError(ex, inputPath);
            return new TranscodingResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private static bool IsFFmpegAvailable()
    {
        if (_ffmpegAvailable.HasValue)
        {
            return _ffmpegAvailable.Value;
        }

        lock (ProbeLock)
        {
            if (_ffmpegAvailable.HasValue)
            {
                return _ffmpegAvailable.Value;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = Process.Start(startInfo);
                process?.WaitForExit(10_000);
                _ffmpegAvailable = process is { ExitCode: 0 };
            }
            catch (Exception)
            {
                _ffmpegAvailable = false;
            }

            return _ffmpegAvailable.Value;
        }
    }

    private static async Task<int?> ProbeHeightAsync(string inputPath, CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v error -select_streams v:0 -show_entries stream=height -of csv=p=0 \"{inputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            return int.TryParse(output.Trim(), out var height) ? height : null;
        }
        catch (Exception)
        {
            return null;
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

        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"FFmpeg exited with code {process.ExitCode}: {error}");
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting transcoding for {InputPath} to {OutputDirectory}")]
    private partial void LogTranscodingStarted(string inputPath, string outputDirectory);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transcoding completed for {InputPath}")]
    private partial void LogTranscodingCompleted(string inputPath);

    [LoggerMessage(Level = LogLevel.Warning, Message = "ffmpeg not available on this host; skipping transcoding for {InputPath}")]
    private partial void LogFFmpegUnavailable(string inputPath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error during transcoding of {InputPath}")]
    private partial void LogTranscodingError(Exception ex, string inputPath);
}
