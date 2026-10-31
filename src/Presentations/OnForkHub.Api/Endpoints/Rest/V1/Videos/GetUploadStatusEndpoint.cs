namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for getting the status of a video upload.
/// </summary>
public sealed partial class GetUploadStatusEndpoint(ILogger<GetUploadStatusEndpoint> logger, IVideoUploadService uploadService, OnForkHub.Core.Interfaces.Repositories.IVideoUploadRepository uploads) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/upload/{{uploadId}}/status";

    private readonly ILogger<GetUploadStatusEndpoint> _logger = logger;

    private readonly IVideoUploadService _uploadService = uploadService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapGet(
                Route,
                [Authorize]
                async ([FromRoute] Guid uploadId, HttpContext context, CancellationToken cancellationToken) =>
                {
                    return await HandleGetStatusAsync(uploadId, context, cancellationToken);
                }
            )
            .WithName("GetVideoUploadStatusV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video Upload")
            .WithDescription("Gets the current status and progress of a video upload")
            .WithSummary("Get upload status")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<VideoUploadResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting upload status for {UploadId}")]
    private partial void LogGettingStatus(Guid uploadId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to get status for upload {UploadId}: {Message}")]
    private partial void LogGetStatusFailed(Guid uploadId, string? message);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Get upload status was cancelled for {UploadId}")]
    private partial void LogGetStatusCancelled(Guid uploadId);

    private async Task<IResult> HandleGetStatusAsync(Guid uploadId, HttpContext context, CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var upload = await uploads.GetByIdAsync(uploadId);
            if (upload.Status != EResultStatus.Success || upload.Data is null)
            {
                return Results.NotFound(new { error = "Upload not found", uploadId });
            }

            if (!string.Equals(upload.Data.UserId.ToString(), userId, StringComparison.OrdinalIgnoreCase))
            {
                return Results.Forbid();
            }

            LogGettingStatus(uploadId);

            var statusResult = await _uploadService.GetUploadStatusAsync(uploadId);

            if (statusResult.Status != EResultStatus.Success)
            {
                if (statusResult.Status == EResultStatus.EntityNotFound)
                {
                    return Results.NotFound(new { error = "Upload not found", uploadId });
                }

                LogGetStatusFailed(uploadId, statusResult.Message);

                return Results.BadRequest(new { error = statusResult.Message ?? "Failed to get upload status" });
            }

            return Results.Ok(new { uploadId, status = statusResult.Data.ToString() });
        }
        catch (OperationCanceledException)
        {
            LogGetStatusCancelled(uploadId);
            return Results.StatusCode(499);
        }
    }
}
