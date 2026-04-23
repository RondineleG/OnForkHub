namespace OnForkHub.Core.Requests.Videos;

/// <summary>
/// Request to create a new comment.
/// </summary>
public record CreateCommentRequest(string Content, Guid? ParentCommentId = null);
