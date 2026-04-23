namespace OnForkHub.Core.Entities;

using OnForkHub.Core.Interfaces.Validations;

/// <summary>
/// Represents a comment on a video.
/// </summary>
public class Comment : BaseEntity
{
    protected Comment(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    private Comment() { }

    public Guid VideoId { get; private set; }

    public Id UserId { get; private set; } = null!;

    public string Content { get; private set; } = string.Empty;

    public Guid? ParentCommentId { get; private set; }

    public bool IsEdited { get; private set; }

    /// <summary>
    /// Creates a new comment.
    /// </summary>
    /// <returns></returns>
    public static RequestResult<Comment> Create(Guid videoId, Id userId, string content, Guid? parentCommentId = null)
    {
        try
        {
            var comment = new Comment
            {
                VideoId = videoId,
                UserId = userId,
                Content = content ?? throw new ArgumentNullException(nameof(content)),
                ParentCommentId = parentCommentId,
                IsEdited = false,
            };

            comment.ValidateEntityState();
            return RequestResult<Comment>.Success(comment);
        }
        catch (DomainException ex)
        {
            return RequestResult<Comment>.WithError(ex.Message);
        }
    }

    /// <summary>
    /// Edits the comment content.
    /// </summary>
    public void Edit(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
        {
            throw new DomainException("Comment content cannot be empty.");
        }

        Content = newContent;
        IsEdited = true;
        Update();
    }

    protected override void ValidateEntityState()
    {
        base.ValidateEntityState();

        var result = ValidationResult.Success();

        result.AddErrorIf(() => string.IsNullOrWhiteSpace(Content), "Content is required.", nameof(Content));
        result.AddErrorIf(() => Content.Length > 1000, "Content cannot exceed 1000 characters.", nameof(Content));
        result.AddErrorIf(() => UserId == null, "UserId is required.", nameof(UserId));
        result.AddErrorIf(() => VideoId == Guid.Empty, "VideoId is required.", nameof(VideoId));

        if (result.HasError)
        {
            throw new DomainException(result.ErrorMessage);
        }
    }
}
