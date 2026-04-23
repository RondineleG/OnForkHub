namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for getting the list of video uploads for a user.
/// </summary>
public sealed partial class GetUserUploadsEndpoint(ILogger<GetUserUploadsEndpoint> logger, IVideoUploadService uploadService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/uploads";

    private readonly ILogger<GetUserUploadsEndpoint> _logger = logger;

    private readonly IVideoUploadService _uploadService = uploadService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapGet(
                Route,
                [Authorize]
                async (
                    [FromQuery] string userId,
                    [FromQuery] int page = 1,
                    [FromQuery] int pageSize = 20,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    return await HandleGetUserUploadsAsync(userId, page, pageSize, cancellationToken);
                }
            )
            .WithName("GetUserVideoUploadsV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video Upload")
            .WithDescription("Gets the list of video uploads for a specific user with pagination")
            .WithSummary("Get user uploads")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<IReadOnlyList<VideoUploadResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting uploads for user {UserId}, page {Page}, pageSize {PageSize}")]
    private partial void LogGettingUploads(string userId, int page, int pageSize);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to get uploads for user {UserId}: {Message}")]
    private partial void LogGetUploadsFailed(string userId, string? message);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Get user uploads was cancelled for user {UserId}")]
    private partial void LogGetUploadsCancelled(string userId);

    private async Task<IResult> HandleGetUserUploadsAsync(string userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.BadRequest(new { error = "UserId is required" });
        }

        if (page < 1)
        {
            return Results.BadRequest(new { error = "Page must be at least 1" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return Results.BadRequest(new { error = "PageSize must be between 1 and 100" });
        }

        try
        {
            LogGettingUploads(userId, page, pageSize);

            var result = await _uploadService.GetUserUploadsAsync(userId, page, pageSize);

            if (result.Status != EResultStatus.Success)
            {
                LogGetUploadsFailed(userId, result.Message);

                return Results.BadRequest(new { error = result.Message ?? "Failed to get uploads" });
            }

            return Results.Ok(result.Data ?? []);
        }
        catch (OperationCanceledException)
        {
            LogGetUploadsCancelled(userId);
            return Results.StatusCode(499);
        }
    }
}
