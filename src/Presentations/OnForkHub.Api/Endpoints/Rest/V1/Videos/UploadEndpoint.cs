namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Application.Dtos.Video.Response;
using OnForkHub.Core.Interfaces.Configuration;
using OnForkHub.Core.ValueObjects;

/// <summary>
/// Endpoint for uploading video files.
/// </summary>
public sealed partial class UploadEndpoint(ILogger<UploadEndpoint> logger, IVideoService videoService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/video/upload";

    private readonly ILogger<UploadEndpoint> _logger = logger;

    private readonly IVideoService _videoService = videoService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                [Authorize]
                async (
                    IFormFile file,
                    [FromForm] string title,
                    [FromForm] string description,
                    [FromForm] string userId,
                    CancellationToken cancellationToken
                ) =>
                {
                    return await HandleUploadAsync(file, title, description, userId, cancellationToken);
                }
            )
            .WithName("UploadVideoV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video")
            .WithDescription("Uploads a video file and creates a video record")
            .WithSummary("Upload video")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<VideoResponseDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .DisableAntiforgery();

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Video upload started for user: {UserId}, file: {FileName}")]
    private partial void LogUploadStarted(string userId, string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Video upload completed successfully for user: {UserId}, video ID: {VideoId}")]
    private partial void LogUploadCompleted(string userId, string videoId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Video upload failed for user: {UserId}, reason: {Reason}")]
    private partial void LogUploadFailed(string userId, string reason);

    private async Task<IResult> HandleUploadAsync(
        IFormFile? file,
        string title,
        string description,
        string userId,
        CancellationToken cancellationToken
    )
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file uploaded" });
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Results.BadRequest(new { error = "Title is required" });
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Results.BadRequest(new { error = "Description is required" });
        }

        if (!Guid.TryParse(userId, out var userGuid))
        {
            return Results.BadRequest(new { error = "Invalid user ID format" });
        }

        LogUploadStarted(userId, file.FileName);

        try
        {
            using var stream = file.OpenReadStream();
            Id userIdValue = userId;

            var result = await _videoService.UploadAsync(stream, file.FileName, file.ContentType, title, description, userIdValue, cancellationToken);

            if (result.Status != EResultStatus.Success || result.Data is null)
            {
                LogUploadFailed(userId, result.Message ?? "Unknown error");
                return Results.BadRequest(new { error = result.Message ?? "Failed to upload video" });
            }

            LogUploadCompleted(userId, result.Data.Id.ToString());

            var response = VideoResponseDto.FromVideo(result.Data);
            return Results.Created($"/api/v{V1}/video/{result.Data.Id}", response);
        }
        catch (OperationCanceledException)
        {
            LogUploadFailed(userId, "Operation cancelled");
            return Results.StatusCode(499);
        }
    }
}
