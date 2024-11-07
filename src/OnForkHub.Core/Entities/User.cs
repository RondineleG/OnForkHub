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
        DomainException.ThrowErrorWhen(() => video == null, UserResources.AddVideo);
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
        var validationResult = Name.Validate();
        return validationResult;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
