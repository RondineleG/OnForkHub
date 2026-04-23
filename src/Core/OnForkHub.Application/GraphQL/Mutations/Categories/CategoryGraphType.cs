using GraphQL.Types;

namespace OnForkHub.Application.GraphQL.Mutations.Categories;

public class CategoryGraphType : ObjectGraphType<Category>
{
    public CategoryGraphType()
    {
        Field(x => x.Id);
        Field(x => x.Name);
    }
}
