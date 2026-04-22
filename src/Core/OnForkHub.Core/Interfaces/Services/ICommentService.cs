namespace OnForkHub.Core.Interfaces.Services;

using OnForkHub.Core.Entities;

/// <summary>
/// Service contract for comment operations.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Creates a new comment.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<RequestResult<Comment>> CreateAsync(Guid videoId, Id userId, string content, Guid? parentId = null);

    /// <summary>
    /// Gets paginated comments for a video.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<RequestResult<(IEnumerable<Comment> Items, int TotalCount)>> GetByVideoIdAsync(Guid videoId, int page, int pageSize);

    /// <summary>
    /// Deletes a comment.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<RequestResult> DeleteAsync(Id id, Id userId);
}
