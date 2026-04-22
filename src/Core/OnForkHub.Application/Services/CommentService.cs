namespace OnForkHub.Application.Services;

using Microsoft.Extensions.Logging;

using OnForkHub.Application.Services.Base;
using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service implementation for comment operations.
/// </summary>
public sealed class CommentService(ICommentRepository commentRepository, ILogger<CommentService> logger) : BaseService, ICommentService
{
    private readonly ICommentRepository _commentRepository = commentRepository;
    private readonly ILogger<CommentService> _logger = logger;

    /// <inheritdoc/>
    public async Task<RequestResult<Comment>> CreateAsync(Guid videoId, Id userId, string content, Guid? parentId = null)
    {
        return await ExecuteAsync(async () =>
        {
            var commentResult = Comment.Create(videoId, userId, content, parentId);
            if (commentResult.Status != EResultStatus.Success)
                return commentResult;

            return await _commentRepository.CreateAsync(commentResult.Data!);
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult<(IEnumerable<Comment> Items, int TotalCount)>> GetByVideoIdAsync(Guid videoId, int page, int pageSize)
    {
        return await ExecuteAsync(async () =>
        {
            return await _commentRepository.GetByVideoIdAsync(videoId, page, pageSize);
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult> DeleteAsync(Id id, Id userId)
    {
        return await ExecuteAsync(async () =>
        {
            // In a real scenario, we check if user owns the comment or is admin
            return await _commentRepository.DeleteAsync(id);
        });
    }
}
