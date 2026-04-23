namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for initiating a chunked video upload.
/// </summary>
public sealed partial class InitiateUploadEndpoint(ILogger<InitiateUploadEndpoint> logger, IVideoUploadService uploadService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/upload/initiate";

    private readonly ILogger<InitiateUploadEndpoint> _logger = logger;

    private readonly IVideoUploadService _uploadService = uploadService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                async ([FromBody] InitiateUploadRequest request, CancellationToken cancellationToken) =>
                {
                    return await HandleInitiateAsync(request, cancellationToken);
                }
            )
            .WithName("InitiateVideoUploadV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video Upload")
            .WithDescription("Initiates a new chunked video upload and returns the upload ID")
            .WithSummary("Initiate video upload")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<VideoUploadResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Initiating chunked upload for user {UserId}, file {FileName}, size {FileSize}, chunks {TotalChunks}"
    )]
    private partial void LogInitiateStarted(string userId, string fileName, long fileSize, int totalChunks);

    [LoggerMessage(Level = LogLevel.Information, Message = "Upload initiated successfully for user {UserId}, upload ID: {UploadId}")]
    private partial void LogInitiateCompleted(string userId, string uploadId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to initiate upload for user {UserId}: {Message}")]
    private partial void LogInitiateFailed(string userId, string message);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Upload initiation was cancelled for user {UserId}")]
    private partial void LogInitiateCancelled(string userId);

    private async Task<IResult> HandleInitiateAsync(InitiateUploadRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                LogInitiateStarted(request.UserId, request.FileName, request.FileSize, request.TotalChunks);
            }

            var result = await _uploadService.InitiateUploadAsync(request.FileName, request.FileSize, request.ContentType, request.UserId);

            if (result.Status != EResultStatus.Success || result.Data is null)
            {
                LogInitiateFailed(request.UserId, result.Message ?? "Unknown error");
                return Results.BadRequest(new { error = result.Message ?? "Failed to initiate upload" });
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                LogInitiateCompleted(request.UserId, result.Data.Id);
            }

            return Results.Created($"/api/v{V1}/videos/upload/{result.Data.Id}/status", result.Data);
        }
        catch (OperationCanceledException)
        {
            LogInitiateCancelled(request.UserId);
            return Results.StatusCode(499);
        }
    }
}
