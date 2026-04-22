namespace OnForkHub.Application.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Background service for processing video uploads (thumbnails, metadata, transcoding, etc.).
/// </summary>
public partial class VideoProcessingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<VideoProcessingBackgroundService> logger)
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<VideoProcessingBackgroundService> _logger = logger;

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogServiceStarting();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingUploadsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                LogProcessingError(ex);
            }

            // Wait for 30 seconds before next check
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        LogServiceStopping();
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting transcoding for upload {UploadId}...")]
    private partial void LogTranscodingStarted(string uploadId);

    private async Task ProcessPendingUploadsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IVideoUploadRepository>();
        var transcodingService = scope.ServiceProvider.GetRequiredService<IVideoTranscodingService>();
        var videoService = scope.ServiceProvider.GetRequiredService<IVideoService>();

        // Get uploads for processing
        var result = await repository.GetByUserIdAsync(string.Empty, 1, 100);

        if (result.Status != EResultStatus.Success || result.Data == null)
        {
            return;
        }

        var pendingProcessing = result.Data.Where(x => x.Status == EVideoUploadStatus.Processing);

        foreach (var upload in pendingProcessing)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                LogProcessingUpload(upload.Id);

                // 1. Transcoding
                if (upload.StoragePath != null)
                {
                    LogTranscodingStarted(upload.Id);
                    var outputDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(upload.StoragePath) ?? "videos", "transcoded", upload.Id);

                    var transResult = await transcodingService.TranscodeToAdaptiveBitrateAsync(upload.StoragePath, outputDir, stoppingToken);

                    if (!transResult.Success)
                    {
                        throw new Exception($"Transcoding failed: {transResult.ErrorMessage}");
                    }

                    // Update thumbnail if generated
                    if (!string.IsNullOrEmpty(transResult.ThumbnailPath))
                    {
                        var videoResult = await videoService.GetByIdAsync(upload.Id);
                        if (videoResult.Status == EResultStatus.Success && videoResult.Data != null)
                        {
                            videoResult.Data.UpdateThumbnail("/videos/thumbnails/" + System.IO.Path.GetFileName(transResult.ThumbnailPath));
                            await videoService.UpdateAsync(videoResult.Data);
                        }
                    }
                }

                // 2. Mark as Completed
                upload.MarkAsCompleted("/videos/final/" + upload.FileName);
                await repository.UpdateAsync(upload);

                LogUploadProcessed(upload.Id);
            }
            catch (Exception ex)
            {
                LogUploadFailed(ex, upload.Id);
                upload.MarkAsFailed(ex.Message);
                await repository.UpdateAsync(upload);
            }
        }
    }
}
