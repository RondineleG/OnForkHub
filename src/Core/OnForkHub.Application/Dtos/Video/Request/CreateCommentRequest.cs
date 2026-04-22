namespace OnForkHub.Application.Dtos.Video.Request;

/// <summary>
/// Request to create a new comment.
/// </summary>
/// <param name="Content">The comment text content.</param>
/// <param name="ParentCommentId">Optional parent comment identifier for replies.</param>
public record CreateCommentRequest(string Content, Guid? ParentCommentId = null);
