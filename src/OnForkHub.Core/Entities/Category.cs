namespace OnForkHub.Core.Entities;

public class Category : BaseEntity
{
    private Category() { }

    public string Description { get; private set; } = string.Empty;

    public Name Name { get; private set; } = null!;

    public static RequestResult<Category> Create(Name name, string description)
    {
        var category = new Category { Name = name, Description = description };

        var validationResult = category.Validate();
        return validationResult.Errors.Count > 0
            ? RequestResult<Category>.WithError(validationResult.ErrorMessage)
            : RequestResult<Category>.Success(category);
    }

    public static RequestResult<Category> Load(
        long id,
        Name name,
        string description,
        DateTime createdAt,
        DateTime? updatedAt = null
    )
    {
        var category = new Category { Name = name, Description = description };

        var validationResult = category.Validate();
        if (validationResult.Errors.Count > 0)
        {
            return RequestResult<Category>.WithError(validationResult.ErrorMessage);
        }

        category.SetId(id, createdAt, updatedAt);
        return RequestResult<Category>.Success(category);
    }

    public RequestResult UpdateCategory(Name name, string description)
    {
        Name = name;
        Description = description;

        var validationResult = Validate();
        if (validationResult.Errors.Count > 0)
        {
            return RequestResult.WithError(validationResult.ErrorMessage);
        }

        Update();
        return RequestResult.Success();
    }

    public override ValidationResult Validate()
    {
        var validationResult = Name.Validate();
        return validationResult;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        DomainException.ThrowErrorWhen(() => id <= 0, BaseEntityResources.IdGreaterThanZero);
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
