namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

/// <summary>
/// Metadata supplied when publishing a stored video.
/// </summary>
public sealed record PublishUploadedVideoRequest(string Title, string Description);
