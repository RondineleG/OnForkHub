namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Streams a completed upload only to its owner, with HTTP range support.
/// </summary>
public sealed class StreamUploadedVideoEndpoint(
    IVideoUploadRepository uploads,
    IFileStorageService storage) : IEndpointAsync
{
    private const int V1 = 1;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var versions = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapGet(
                $"/api/v{V1}/videos/upload/{{uploadId:guid}}/stream",
                [Authorize]
                async (Guid uploadId, HttpContext context) =>
                {
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? context.User.FindFirstValue("sub");
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        return Results.Unauthorized();
                    }

                    var result = await uploads.GetByIdAsync(uploadId);
                    if (result.Data is null)
                    {
                        return Results.NotFound();
                    }

                    var upload = result.Data;
                    if (!string.Equals(upload.UserId.ToString(), userId, StringComparison.OrdinalIgnoreCase))
                    {
                        return Results.Forbid();
                    }

                    if (upload.Status != EVideoUploadStatus.Completed || string.IsNullOrWhiteSpace(upload.StoragePath))
                    {
                        return Results.Conflict(new { error = "Upload is not ready." });
                    }

                    var file = await storage.GetAsync(upload.StoragePath);
                    if (file.Data is null)
                    {
                        return Results.NotFound();
                    }

                    context.Response.Headers.CacheControl = "private, no-store";
                    return Results.File(
                        file.Data,
                        contentType: upload.ContentType,
                        enableRangeProcessing: true);
                })
            .WithName("StreamUploadedVideoV1")
            .WithApiVersionSet(versions)
            .MapToApiVersion(V1)
            .WithTags("Video Upload")
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
