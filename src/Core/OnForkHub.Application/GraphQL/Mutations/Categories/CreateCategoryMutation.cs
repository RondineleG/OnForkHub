namespace OnForkHub.Application.GraphQL.Mutations.Categories;

public class CreateCategoryMutation : MutationGraphQLBase
{
    public static async Task<RequestResult<Category>> HandleAsync(CategoryRequestDto input, [Service] IUseCase<CategoryRequestDto, Category> useCase)
    {
        return await useCase.ExecuteAsync(input);
    }

    public override void Register(IObjectTypeDescriptor descriptor)
    {
        descriptor
            .Field("createCategory")
            .Argument("input", a => a.Type<NonNullType<InputObjectType<CategoryRequestDto>>>())
            .ResolveWith<CreateCategoryMutation>(m => HandleAsync(default!, default!))
            .Type<ObjectType<RequestResult<Category>>>()
            .Description("Creates a new category");
    }
}