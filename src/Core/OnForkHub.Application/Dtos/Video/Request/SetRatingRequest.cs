namespace OnForkHub.Application.Dtos.Video.Request;

using OnForkHub.Core.Enums;

/// <summary>
/// Request to set a rating for a video.
/// </summary>
/// <param name="Type">The type of rating (Like/Dislike).</param>
public record SetRatingRequest(ERatingType Type);
