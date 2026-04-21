namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for uploading a chunk of a video file.
/// </summary>
public sealed partial class UploadChunkEndpoint(ILogger<UploadChunkEndpoint> logger, IVideoUploadService uploadService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/upload/chunk/{{uploadId}}";

    private readonly ILogger<UploadChunkEndpoint> _logger = logger;

    private readonly IVideoUploadService _uploadService = uploadService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                [Authorize]
                async (
                    [FromRoute] Guid uploadId,
                    IFormFile chunk,
                    [FromForm] int chunkIndex,
                    [FromForm] int totalChunks,
                    CancellationToken cancellationToken
                ) =>
                {
                    return await HandleChunkAsync(uploadId, chunk, chunkIndex, totalChunks, cancellationToken);
                }
            )
            .WithName("UploadVideoChunkV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video Upload")
            .WithDescription("Uploads a chunk of a video file for a previously initiated upload")
            .WithSummary("Upload video chunk")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<VideoUploadResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery()
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing chunk {ChunkIndex}/{TotalChunks} for upload {UploadId}, size {Size} bytes")]
    private partial void LogProcessingChunk(int chunkIndex, int totalChunks, Guid uploadId, long size);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to process chunk {ChunkIndex} for upload {UploadId}: {Message}")]
    private partial void LogProcessChunkFailed(int chunkIndex, Guid uploadId, string? message);

    [LoggerMessage(Level = LogLevel.Information, Message = "Chunk {ChunkIndex}/{TotalChunks} processed for upload {UploadId}. Status: {Status}")]
    private partial void LogChunkProcessed(int chunkIndex, int totalChunks, Guid uploadId, string status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Chunk upload was cancelled for upload {UploadId}")]
    private partial void LogChunkUploadCancelled(Guid uploadId);

    private async Task<IResult> HandleChunkAsync(
        Guid uploadId,
        IFormFile? chunk,
        int chunkIndex,
        int totalChunks,
        CancellationToken cancellationToken
    )
    {
        if (chunk is null || chunk.Length == 0)
        {
            return Results.BadRequest(new { error = "No chunk data provided" });
        }

        if (chunkIndex < 0 || chunkIndex >= totalChunks)
        {
            return Results.BadRequest(new { error = "Invalid chunk index" });
        }

        try
        {
            LogProcessingChunk(chunkIndex, totalChunks, uploadId, chunk.Length);

            using var stream = chunk.OpenReadStream();
            var result = await _uploadService.UploadChunkAsync(uploadId, stream, chunkIndex, totalChunks);

            if (result.Status != EResultStatus.Success)
            {
                LogProcessChunkFailed(chunkIndex, uploadId, result.Message);

                return Results.BadRequest(new { error = result.Message ?? "Failed to process chunk" });
            }

            var statusResult = await _uploadService.GetUploadStatusAsync(uploadId);

            if (statusResult.Status != EResultStatus.Success)
            {
                return Results.BadRequest(new { error = "Failed to retrieve upload status" });
            }

            var statusStr = statusResult.Data.ToString();
            LogChunkProcessed(chunkIndex, totalChunks, uploadId, statusStr);

            return Results.Ok(
                new
                {
                    uploadId,
                    chunkIndex,
                    status = statusStr,
                    isComplete = statusResult.Data == EVideoUploadStatus.Completed,
                }
            );
        }
        catch (OperationCanceledException)
        {
            LogChunkUploadCancelled(uploadId);
            return Results.StatusCode(499);
        }
    }
}
