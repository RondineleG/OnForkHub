namespace OnForkHub.Core.Entities;

public class Category : BaseEntity
{
    protected Category(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    private Category() { }

    public string Description { get; private set; } = string.Empty;

    [JsonConverter(typeof(NameConverter))]
    public Name Name { get; private set; } = null!;

    public static RequestResult<Category> Create(Name name, string description)
    {
        try
        {
            var category = new Category { Name = name, Description = description };
            category.ValidateEntityState();
            return RequestResult<Category>.Success(category);
        }
        catch (DomainException ex)
        {
            return RequestResult<Category>.WithError(ex.Message);
        }
    }

    public static RequestResult<Category> Load(Id id, Name name, string description, DateTime createdAt, DateTime? updatedAt = null)
    {
        try
        {
            var category = new Category(id, createdAt, updatedAt) { Name = name, Description = description };
            category.ValidateEntityState();
            return RequestResult<Category>.Success(category);
        }
        catch (DomainException ex)
        {
            return RequestResult<Category>.WithError(ex.Message);
        }
    }

    public RequestResult UpdateCategory(Name name, string description)
    {
        try
        {
            Name = name;
            Description = description;
            ValidateEntityState();
            Update();
            return RequestResult.Success();
        }
        catch (DomainException ex)
        {
            return RequestResult.WithError(ex.Message);
        }
    }

    protected override string GetCollectionName()
    {
        return "categories";
    }

    protected override void ValidateEntityState()
    {
        base.ValidateEntityState();
        var validationResult = ValidationResult.Success().AddErrorIf(() => Name is null, "Name is required", nameof(Name));
        if (Name is not null)
        {
            validationResult.Merge(Name.Validate());
        }
        validationResult
            .AddErrorIf(() => string.IsNullOrWhiteSpace(Description), "Description is required", nameof(Description))
            .AddErrorIf(() => Description?.Length > 200, "Description cannot exceed 200 characters", nameof(Description));
        if (validationResult.HasError)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }
    }
}