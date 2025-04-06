namespace OnForkHub.Api.Endpoints.GraphQL.Mutations.Categories;

public class CreateCategoryMutation
{
    public async Task<RequestResult<Category>> CreateCategoryAsync(CategoryRequestDto input, [Service] IUseCase<CategoryRequestDto, Category> useCase)
    {
        return await useCase.ExecuteAsync(input);
    }
}
