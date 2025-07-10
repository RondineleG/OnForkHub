using OnForkHub.Core.Interfaces.GraphQL;

namespace OnForkHub.Application.GraphQL.Handlers;

public class CreateCategoryHandler(IUseCase<CategoryRequestDto, Category> useCase) : IGraphQLMutationHandler<CategoryRequestDto, Category>
{
    private readonly IUseCase<CategoryRequestDto, Category> _useCase = useCase;

    public async Task<RequestResult<Category>> HandleAsync(CategoryRequestDto input)
    {
        return await _useCase.ExecuteAsync(input);
    }
}
