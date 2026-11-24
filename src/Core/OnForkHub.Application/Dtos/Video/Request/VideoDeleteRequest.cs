namespace OnForkHub.Application.Dtos.Video.Request;

using System.Text.Json.Serialization;

/// <summary>
/// Request to delete a video, carrying the server-populated requester context for ownership validation.
/// </summary>
public sealed record VideoDeleteRequest
{
    /// <summary>Gets the identifier of the video to delete.</summary>
    public required string Id { get; init; }

    /// <summary>
    /// Server-populated identifier of the authenticated requester.
    /// </summary>
    [JsonIgnore]
    public string? RequesterUserId { get; init; }

    /// <summary>
    /// Server-populated flag indicating whether the requester has the Admin role.
    /// </summary>
    [JsonIgnore]
    public bool RequesterIsAdmin { get; init; }
}
