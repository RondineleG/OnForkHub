namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Publishes a successfully stored upload as a catalog video.
/// </summary>
public sealed class PublishUploadedVideoEndpoint(
    IVideoUploadRepository uploads,
    IVideoRepositoryEF videoRepository,
    IVideoService videos) : IEndpointAsync
{
    private const int V1 = 1;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var versions = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();
        app.MapPost(
                $"/api/v{V1}/videos/upload/{{uploadId:guid}}/publish",
                [Authorize]
                async (Guid uploadId, PublishUploadedVideoRequest request, HttpContext context) =>
                {
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? context.User.FindFirstValue("sub");
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        return Results.Unauthorized();
                    }

                    var uploadResult = await uploads.GetByIdAsync(uploadId);
                    if (uploadResult.Status != EResultStatus.Success || uploadResult.Data is null)
                    {
                        return Results.NotFound(new { error = "Upload not found." });
                    }

                    var upload = uploadResult.Data;
                    if (!string.Equals(upload.UserId.ToString(), userId, StringComparison.OrdinalIgnoreCase))
                    {
                        return Results.Forbid();
                    }

                    if (upload.Status != EVideoUploadStatus.Completed || string.IsNullOrWhiteSpace(upload.StoragePath))
                    {
                        return Results.Conflict(new { error = "The upload is not stored yet." });
                    }

                    var existing = await videoRepository.GetByUploadIdAsync(uploadId);
                    if (existing.Status == EResultStatus.Success && existing.Data is not null)
                    {
                        return Results.Ok(VideoResponse.FromVideo(existing.Data));
                    }

                    var created = Video.Create(request.Title, request.Description, upload.StoragePath, upload.UserId);
                    if (created.Status != EResultStatus.Success || created.Data is null)
                    {
                        return Results.BadRequest(new { error = created.Message ?? "Invalid video metadata." });
                    }

                    created.Data.AssociateUpload(uploadId);
                    var result = await videos.CreateAsync(created.Data);
                    if (result.Status != EResultStatus.Success || result.Data is null)
                    {
                        return Results.BadRequest(new { error = result.Message ?? "Unable to publish the video." });
                    }

                    return Results.Created($"/api/v{V1}/videos/{result.Data.Id}", VideoResponse.FromVideo(result.Data));
                })
            .WithName("PublishUploadedVideoV1")
            .WithApiVersionSet(versions)
            .MapToApiVersion(V1)
            .WithTags("Video Upload")
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
