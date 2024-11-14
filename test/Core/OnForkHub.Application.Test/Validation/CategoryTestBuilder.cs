namespace OnForkHub.Application.Test.Validation;

public class CategoryTestBuilder
{
    private Name _name = Name.Create("Test Name");
    private string _description = "Test Description";

    public CategoryTestBuilder WithName(Name name)
    {
        _name = name;
        return this;
    }

    public CategoryTestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Category Build()
    {
        var result = Category.Create(_name, _description);
        if (!result.Status.Equals(EResultStatus.Success))
            throw new InvalidOperationException($"Failed to build category: {result.Message}");

        return result.Data!;
    }
}
