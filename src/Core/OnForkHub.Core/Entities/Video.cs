// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Entities;

public class Video : BaseEntity
{
    public Video(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    protected Video() { }

    private readonly List<Category> _categories = [];

    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public string Description { get; private set; } = string.Empty;

    public Title Title { get; private set; } = null!;

    public Url Url { get; private set; } = null!;

    public Id? UserId { get; private set; }

    public static RequestResult<Video> Create(string title, string description, string url, Id userId)
    {
        try
        {
            var video = new Video
            {
                Title = Title.Create(title),
                Description = description ?? throw new ArgumentNullException(nameof(description)),
                Url = Url.Create(url),
                UserId = userId,
            };

            video.ValidateEntityState();
            return RequestResult<Video>.Success(video);
        }
        catch (DomainException ex)
        {
            return RequestResult<Video>.WithError(ex.Message);
        }
    }

    public static RequestResult<Video> Load(
        Id id,
        string title,
        string description,
        string url,
        Id userId,
        DateTime createdAt,
        DateTime? updatedAt = null
    )
    {
        try
        {
            var video = new Video(id, createdAt, updatedAt)
            {
                Title = Title.Create(title),
                Description = description ?? throw new ArgumentNullException(nameof(description)),
                Url = Url.Create(url),
                UserId = userId,
            };

            video.ValidateEntityState();
            return RequestResult<Video>.Success(video);
        }
        catch (DomainException ex)
        {
            return RequestResult<Video>.WithError(ex.Message);
        }
    }

    public RequestResult AddCategory(Category category)
    {
        try
        {
            ValidationResult.Success().AddErrorIf(() => category is null, VideoResources.CategoryCannotBeNull).ThrowIfInvalid();

            if (!_categories.Contains(category))
            {
                _categories.Add(category);
                Update();
            }

            return RequestResult.Success();
        }
        catch (DomainException ex)
        {
            return RequestResult.WithError(ex.Message);
        }
    }

    public RequestResult RemoveCategory(Category category)
    {
        try
        {
            ValidationResult.Success().AddErrorIf(() => category is null, VideoResources.CategoryCannotBeNull).ThrowIfInvalid();

            if (_categories.Remove(category))
            {
                Update();
            }

            return RequestResult.Success();
        }
        catch (DomainException ex)
        {
            return RequestResult.WithError(ex.Message);
        }
    }

    public RequestResult UpdateVideo(string title, string description, string url)
    {
        try
        {
            Title = Title.Create(title);
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Url = Url.Create(url);

            ValidateEntityState();
            Update();
            return RequestResult.Success();
        }
        catch (DomainException ex)
        {
            return RequestResult.WithError(ex.Message);
        }
    }

    protected override void ValidateEntityState()
    {
        base.ValidateEntityState();

        var validationResult = ValidationResult.Success();

        validationResult
            .AddErrorIf(() => string.IsNullOrWhiteSpace(Description), VideoResources.DescriptionRequired, nameof(Description))
            .AddErrorIf(() => Description.Length < 5, VideoResources.DescriptionMinLength, nameof(Description))
            .AddErrorIf(() => Description.Length > 200, VideoResources.DescriptionMaxLength, nameof(Description))
            .AddErrorIf(() => UserId == null, VideoResources.UserIdRequired, nameof(UserId));

        if (Title != null)
        {
            validationResult.Merge(Title.Validate());
        }
        else
        {
            validationResult.AddError("Title is required", nameof(Title));
        }

        if (Url != null)
        {
            validationResult.Merge(Url.Validate());
        }
        else
        {
            validationResult.AddError("Url is required", nameof(Url));
        }

        if (validationResult.HasError)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }
    }
}
