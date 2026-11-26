namespace OnForkHub.Application.Services;

using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Consumes the video processing queue: downloads each stored raw upload, runs
/// transcoding/thumbnail generation and marks the upload Completed (publishable) or Failed.
/// Uploads left in Processing by a crash are re-enqueued once at startup.
/// </summary>
public partial class VideoProcessingBackgroundService(IServiceScopeFactory scopeFactory, IVideoProcessingChannel queue, ILogger<VideoProcessingBackgroundService> logger)
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IVideoProcessingChannel _queue = queue;
    private readonly ILogger<VideoProcessingBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogServiceStarting();

        await ReenqueueInterruptedUploadsAsync(stoppingToken);

        try
        {
            await foreach (var uploadId in _queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessUploadAsync(uploadId, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    LogProcessingError(ex);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown.
        }

        LogServiceStopping();
    }

    private async Task ReenqueueInterruptedUploadsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IVideoUploadRepository>();

            var page = 1;
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await repository.GetByStatusAsync(EVideoUploadStatus.Processing, page, 100);
                if (result.Status != EResultStatus.Success || result.Data is null)
                {
                    break;
                }

                var items = result.Data.ToList();
                if (items.Count == 0)
                {
                    break;
                }

                foreach (var upload in items)
                {
                    if (Guid.TryParse(upload.Id, out var interruptedId))
                    {
                        await _queue.EnqueueAsync(interruptedId, stoppingToken);
                    }
                }

                if (items.Count < 100)
                {
                    break;
                }

                page++;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogProcessingError(ex);
        }
    }

    private async Task ProcessUploadAsync(Guid uploadId, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IVideoUploadRepository>();
        var transcodingService = scope.ServiceProvider.GetRequiredService<IVideoTranscodingService>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        var uploadResult = await repository.GetByIdAsync(uploadId);
        if (uploadResult.Status != EResultStatus.Success || uploadResult.Data is null)
        {
            return;
        }

        var upload = uploadResult.Data;
        if (upload.Status != EVideoUploadStatus.Processing || string.IsNullOrWhiteSpace(upload.StoragePath))
        {
            // Already processed, failed or never stored: nothing to do.
            return;
        }

        LogProcessingUpload(upload.Id);

        var workDir = Path.Combine(Path.GetTempPath(), "onforkhub-processing", uploadId.ToString("N"));
        try
        {
            Directory.CreateDirectory(workDir);
            var inputPath = Path.Combine(workDir, "input" + Path.GetExtension(upload.FileName));

            var fileResult = await storage.GetAsync(upload.StoragePath, stoppingToken);
            if (fileResult.Status != EResultStatus.Success || fileResult.Data is null)
            {
                throw new InvalidOperationException($"Stored file not found for upload {upload.Id}.");
            }

            await using (var source = fileResult.Data)
            await using (var destination = new FileStream(inputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await source.CopyToAsync(destination, stoppingToken);
            }

            var outputDir = Path.Combine(workDir, "output");
            var transcode = await transcodingService.TranscodeToAdaptiveBitrateAsync(inputPath, outputDir, stoppingToken);

            if (!transcode.Success)
            {
                throw new InvalidOperationException($"Transcoding failed: {transcode.ErrorMessage}");
            }

            string? thumbnailUrl = null;
            if (!string.IsNullOrWhiteSpace(transcode.ThumbnailPath) && File.Exists(transcode.ThumbnailPath))
            {
                await using var thumbStream = new FileStream(transcode.ThumbnailPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var storedThumb = await storage.UploadAsync(thumbStream, Path.GetFileName(transcode.ThumbnailPath), "image/jpeg", stoppingToken);
                if (storedThumb.Status == EResultStatus.Success)
                {
                    thumbnailUrl = storedThumb.Data;
                }
            }

            var variantUrls = new Dictionary<string, string>();
            foreach (var (name, path) in transcode.Variants)
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                await using var variantStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var storedVariant = await storage.UploadAsync(variantStream, Path.GetFileName(path), "video/mp4", stoppingToken);
                if (storedVariant.Status == EResultStatus.Success && !string.IsNullOrWhiteSpace(storedVariant.Data))
                {
                    variantUrls[name] = storedVariant.Data;
                }
            }

            var variantsJson = variantUrls.Count > 0 ? JsonSerializer.Serialize(variantUrls) : null;
            upload.MarkAsCompleted(upload.StoragePath, thumbnailUrl, variantsJson);
            await repository.UpdateAsync(upload);

            LogUploadProcessed(upload.Id);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogUploadFailed(ex, upload.Id);
            upload.MarkAsFailed(ex.Message);
            await repository.UpdateAsync(upload);
        }
        finally
        {
            try
            {
                if (Directory.Exists(workDir))
                {
                    Directory.Delete(workDir, recursive: true);
                }
            }
            catch (IOException)
            {
                // Temp cleanup is best-effort.
            }
            catch (UnauthorizedAccessException)
            {
                // Temp cleanup is best-effort.
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Video Processing Background Service is starting.")]
    private partial void LogServiceStarting();

    [LoggerMessage(Level = LogLevel.Information, Message = "Video Processing Background Service is stopping.")]
    private partial void LogServiceStopping();

    [LoggerMessage(Level = LogLevel.Error, Message = "Error occurred while processing video uploads.")]
    private partial void LogProcessingError(Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing video upload {UploadId}...")]
    private partial void LogProcessingUpload(string uploadId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Video upload {UploadId} processed successfully.")]
    private partial void LogUploadProcessed(string uploadId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process upload {UploadId}")]
    private partial void LogUploadFailed(Exception ex, string uploadId);
}
