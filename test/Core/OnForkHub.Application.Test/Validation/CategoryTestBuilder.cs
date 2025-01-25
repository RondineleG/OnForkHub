namespace OnForkHub.Application.Test.Validation;

public class CategoryTestBuilder
{
    private string _description = "Test Description";

    private Name _name = Name.Create("Test Name");

    public Category? Build()
    {
        var result = Category.Create(_name, _description);
        return !result.Status.Equals(EResultStatus.Success)
            ? throw new InvalidOperationException($"Failed to build category: {result.Message}")
            : result.Data;
    }

    public CategoryTestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CategoryTestBuilder WithName(Name name)
    {
        _name = name;
        return this;
    }
}