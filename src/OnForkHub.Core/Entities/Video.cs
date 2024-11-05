using OnForkHub.Core.ValueObjects;

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
        DomainException.ThrowErrorWhen(() => category == null, $"{nameof(Category)} cannot be null");

        if (!_categories.Contains(category))
        {
            _categories.Add(category);
            Update();
        }
    }


    public void RemoveCategory(Category category)
    {
        DomainException.ThrowErrorWhen(() => category == null, $"{nameof(Category)} cannot be null");

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
        validationResult.AddErrorIfNullOrWhiteSpace(Title, $"{nameof(Title)} is required", nameof(Title));
        validationResult.AddErrorIf(
            Title.Length < 3,
            $"{nameof(Title)} must be at least 3 characters long",
            nameof(Title)
        );
        validationResult.AddErrorIf(
            Title.Length > 50,
            $"{nameof(Title)} must be no more than 50 characters",
            nameof(Title)
        );
        validationResult.AddErrorIfNullOrWhiteSpace(
            Description,
            $"{nameof(Description)} is required",
            nameof(Description)
        );
        validationResult.AddErrorIf(
            Description.Length < 5,
            $"{nameof(Description)} must be at least 5 characters",
            nameof(Description)
        );
        validationResult.AddErrorIf(
            Description.Length > 200,
            $"{nameof(Description)} must be no more than 200 characters",
            nameof(Description)
        );
        validationResult.AddErrorIf(UserId <= 0, $"{nameof(UserId)} is required", nameof(UserId));

        return validationResult;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
