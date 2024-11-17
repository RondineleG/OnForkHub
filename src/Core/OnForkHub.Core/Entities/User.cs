namespace OnForkHub.Core.Entities;

public class User : BaseEntity
{
    private readonly List<Video> _videos = [];

    private User()
        : base() { }

    protected User(long id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    public Email Email { get; private set; } = null!;
    public Name Name { get; private set; } = null!;
    public IReadOnlyCollection<Video> Videos => _videos.AsReadOnly();

    public static RequestResult<User> Create(Name name, string email)
    {
        try
        {
            var user = new User { Name = name ?? throw new ArgumentNullException(nameof(name)), Email = Email.Create(email) };

            user.ValidateEntityState();
            return RequestResult<User>.Success(user);
        }
        catch (DomainException ex)
        {
            return RequestResult<User>.WithError(ex.Message);
        }
    }

    public static RequestResult<User> Load(long id, Name name, string email, DateTime createdAt, DateTime? updatedAt = null)
    {
        try
        {
            var user = new User(id, createdAt, updatedAt)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name)),
                Email = Email.Create(email),
            };

            user.ValidateEntityState();
            return RequestResult<User>.Success(user);
        }
        catch (DomainException ex)
        {
            return RequestResult<User>.WithError(ex.Message);
        }
    }

    public RequestResult AddVideo(Video video)
    {
        try
        {
            if (video is null)
            {
                throw new DomainException(UserResources.AddVideo);
            }

            _videos.Add(video);
            Update();
            return RequestResult.Success();
        }
        catch (DomainException ex)
        {
            return RequestResult.WithError(ex.Message);
        }
    }

    public RequestResult UpdateData(Name name, string email)
    {
        try
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Email = Email.Create(email);

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

        var validationResult = new ValidationResult();

        validationResult.AddErrorIfNull(Name, "Name is required", nameof(Name));
        if (Name != null)
        {
            validationResult.Merge(Name.Validate());
        }

        validationResult.AddErrorIfNull(Email, "Email is required", nameof(Email));
        if (Email != null)
        {
            validationResult.Merge(Email.Validate());
        }

        if (validationResult.HasError)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }
    }
}
