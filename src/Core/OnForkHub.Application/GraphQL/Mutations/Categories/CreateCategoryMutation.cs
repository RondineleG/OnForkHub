namespace OnForkHub.Application.GraphQL.Mutations.Categories;

public class CreateCategoryMutation : HotChocolateMutationBase
{
    public override string Name => "createCategory";
    public override string Description => "Creates a new category";

    public static async Task<RequestResult<Category>> HandleAsync(
        CategoryRequestDto input,
        [Service] IUseCase<CategoryRequestDto, Category> useCase)
    {
        return await useCase.ExecuteAsync(input);
    }

    protected override void RegisterMutation(IObjectTypeDescriptor descriptor)
    {
        descriptor
            .Field(Name)
            .Argument("input", a => a.Type<NonNullType<InputObjectType<CategoryRequestDto>>>())
            .ResolveWith<CreateCategoryMutation>(m => HandleAsync(default!, default!))
            .Type<ObjectType<RequestResult<Category>>>()
            .Description(Description);
    }
}