namespace OnForkHub.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Requests;
using OnForkHub.Persistence.Contexts.Base;

/// <summary>
/// EF implementation for comment repository.
/// </summary>
public sealed class CommentRepositoryEF(IEntityFrameworkDataContext context) : ICommentRepository
{
    private readonly IEntityFrameworkDataContext _context = context;

    /// <inheritdoc/>
    public async Task<RequestResult<Comment>> CreateAsync(Comment comment)
    {
        try
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RequestResult<Comment>.Success(comment);
        }
        catch (Exception ex)
        {
            return RequestResult<Comment>.WithError($"Error creating comment: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<(IEnumerable<Comment> Items, int TotalCount)>> GetByVideoIdAsync(Guid videoId, int page, int pageSize)
    {
        try
        {
            var query = _context.Comments.Where(x => x.VideoId == videoId);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return RequestResult<(IEnumerable<Comment>, int)>.Success((items, total));
        }
        catch (Exception ex)
        {
            return RequestResult<(IEnumerable<Comment>, int)>.WithError($"Error fetching comments: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult> DeleteAsync(Id id)
    {
        try
        {
            var comment = await _context.Comments.FindAsync(id.ToString());
            if (comment == null)
                return RequestResult.WithError("Comment not found");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return RequestResult.Success();
        }
        catch (Exception ex)
        {
            return RequestResult.WithError($"Error deleting comment: {ex.Message}");
        }
    }
}
