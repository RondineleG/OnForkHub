namespace OnForkHub.Core.Interfaces.Repositories;

using OnForkHub.Core.Entities;

/// <summary>
/// Repository contract for Comment entity.
/// </summary>
public interface ICommentRepository
{
    /// <summary>
    /// Adds a new comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<RequestResult<Comment>> CreateAsync(Comment comment);

    /// <summary>
    /// Gets paginated comments for a video.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<RequestResult<(IEnumerable<Comment> Items, int TotalCount)>> GetByVideoIdAsync(Guid videoId, int page, int pageSize);

    /// <summary>
    /// Deletes a comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<RequestResult> DeleteAsync(Id id);
}
