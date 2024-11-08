namespace OnForkHub.Core.Entities;

public class Video : BaseEntity
{
    private Video()
    {
        _categories = [];
    }

    private readonly List<Category> _categories;

    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public string Description { get; private set; } = string.Empty;

    public Title Title { get; private set; } = null!;

    public Url Url { get; private set; }

    public long UserId { get; private set; }

    public static Video Create(string title, string description, string url, long userId)
    {
        var video = new Video
        {
            Title = Title.Create(title),
            Description = description,
            Url = Url.Create(url),
            UserId = userId,
        };

        video.Validate();
        return video;
    }

    public static Video Load(
        long id,
        string title,
        string description,
        string url,
        long userId,
        DateTime createdAt,
        DateTime? updatedAt = null
    )
    {
        var video = new Video
        {
            Title = Title.Create(title),
            Description = description,
            Url = Url.Create(url),
            UserId = userId,
        };

        video.SetId(id, createdAt, updatedAt);
        video.Validate();
        return video;
    }

    public void AddCategory(Category category)
    {
        DomainException.ThrowErrorWhen(() => category == null, VideoResources.CategoryCannotBeNull);

        if (!_categories.Contains(category))
        {
            _categories.Add(category);
            Update();
        }
    }

    public void RemoveCategory(Category category)
    {
        DomainException.ThrowErrorWhen(() => category == null, VideoResources.CategoryCannotBeNull);

        if (_categories.Contains(category))
        {
            _categories.Remove(category);
            Update();
        }
    }

    public ValidationResult UpdateCategory(string title, string description, string url)
    {
        Title = Title.Create(title);
        Description = description;
        Url = Url.Create(url);

        var validationResult = Validate();

        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        Update();
        return validationResult;
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(
            Description,
            VideoResources.DescriptionRequired,
            nameof(Description)
        );
        validationResult.AddErrorIf(Description.Length < 5, VideoResources.DescriptionMinLength, nameof(Description));
        validationResult.AddErrorIf(Description.Length > 200, VideoResources.DescriptionMaxLength, nameof(Description));
        validationResult.AddErrorIf(UserId <= 0, VideoResources.UserIdRequired, nameof(UserId));
        validationResult = Title.Validate().Merge(validationResult); //Merge Video validation errors with Title validation errors

        return validationResult;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
