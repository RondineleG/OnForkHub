namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Requests.Videos;
using OnForkHub.Core.ValueObjects;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for managing video comments.
/// </summary>
public sealed partial class CommentEndpoint(ILogger<CommentEndpoint> logger, ICommentService commentService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/{{id:guid}}/comments";

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                [Authorize]
                async ([FromRoute] Guid id, [FromBody] CreateCommentRequest request, ClaimsPrincipal user, CancellationToken cancellationToken) =>
                {
                    return await HandleCreateCommentAsync(id, request, user);
                }
            )
            .WithName("CreateVideoCommentV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Comments")
            .WithDescription("Adds a new comment to a video")
            .WithSummary("Create comment")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapGet(
                Route,
                async ([FromRoute] Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
                {
                    return await HandleGetCommentsAsync(id, page, pageSize);
                }
            )
            .WithName("GetVideoCommentsV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Comments")
            .WithDescription("Gets a paginated list of comments for a video")
            .WithSummary("List comments")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating comment for video {VideoId} by user {UserId}")]
    private static partial void LogCreatingComment(ILogger logger, Guid videoId, string userId);

    private readonly ILogger<CommentEndpoint> _logger = logger;
    private readonly ICommentService _commentService = commentService;

    private async Task<IResult> HandleGetCommentsAsync(Guid videoId, int page, int pageSize)
    {
        var result = await _commentService.GetByVideoIdAsync(videoId, page, pageSize);
        if (result.Status != EResultStatus.Success)
        {
            return Results.BadRequest(result.Message);
        }

        var (items, total) = result.Data;
        return Results.Ok(
            new
            {
                videoId,
                comments = items.Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.UserId,
                    c.CreatedAt,
                    c.IsEdited,
                    c.ParentCommentId,
                }),
                totalCount = total,
            }
        );
    }

    private async Task<IResult> HandleCreateCommentAsync(Guid videoId, CreateCommentRequest request, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        LogCreatingComment(_logger, videoId, userId);

        var result = await _commentService.CreateAsync(videoId, userId, request.Content, request.ParentCommentId);

        if (result.Status != EResultStatus.Success)
        {
            return Results.BadRequest(result.Message);
        }

        return Results.Created($"{Route}", result.Data);
    }
}
