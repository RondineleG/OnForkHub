namespace OnForkHub.Web.Services.Api;

using OnForkHub.Core.Requests.Videos;

/// <summary>
/// Service contract for comment API operations.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Gets comments for a video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A list of comments.</returns>
    Task<List<CommentDisplayModel>> GetCommentsAsync(Guid videoId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Creates a new comment.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <param name="request">The comment request.</param>
    /// <returns>True if created successfully.</returns>
    Task<bool> CreateCommentAsync(Guid videoId, CreateCommentRequest request);
}

/// <summary>
/// Model for displaying comments in the UI.
/// </summary>
public class CommentDisplayModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsEdited { get; set; }
    public List<CommentDisplayModel> Replies { get; set; } = [];
}
