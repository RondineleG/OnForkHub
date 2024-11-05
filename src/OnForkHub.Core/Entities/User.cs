using OnForkHub.Core.ValueObjects;

namespace OnForkHub.Core.Entities;

public class User : BaseEntity
{
    private User()
    {
        _videos = [];
    }

    private readonly List<Video> _videos;

    public Email Email { get; private set; }

    public Name Name { get; private set; } = null!;

    public IReadOnlyCollection<Video> Videos => _videos.AsReadOnly();

    public static User Create(Name name, string email)
    {
        var user = new User { Name = name, Email = Email.Create(email) };

        user.Validate();
        return user;
    }

    public static User Load(long id, Name name, string email, DateTime createdAt, DateTime? updatedAt = null)
    {
        var user = new User { Name = name, Email = Email.Create(email) };

        user.SetId(id, createdAt, updatedAt);
        user.Validate();
        return user;
    }

    public void AddVideo(Video video)
    {
        DomainException.ThrowErrorWhen(() => video == null, $"{nameof(Video)} cannot be null");
        _videos.Add(video);
        Update();
    }

    public void UpdateData(Name name, string email)
    {
        Name = name;
        Email = Email.Create(email);
        Validate();
        Update();
    }

    public void UpdateEmail(string email)
    {
        Email = Email.Create(email);
        Validate();
        Update();
    }

    public void UpdateName(Name name)
    {
        Name = name;
        Validate();
        Update();
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(Name.Value, $"{nameof(Name)} is required", nameof(Name));
        validationResult.AddErrorIf(Name.Value.Length < 3, $"{nameof(Name)} must be at least 3 characters", nameof(Category));
        validationResult.AddErrorIf(
            Name.Value.Length > 50,
            $"{nameof(Name)} must be no more than 50 characters",
            nameof(Category)
        );
        validationResult.ThrowIfInvalid("User name is invalid");
        return validationResult;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
