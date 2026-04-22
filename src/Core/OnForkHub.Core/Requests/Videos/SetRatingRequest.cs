namespace OnForkHub.Core.Requests.Videos;

using OnForkHub.Core.Enums;

/// <summary>
/// Request to set a rating for a video.
/// </summary>
public record SetRatingRequest(ERatingType Type);
