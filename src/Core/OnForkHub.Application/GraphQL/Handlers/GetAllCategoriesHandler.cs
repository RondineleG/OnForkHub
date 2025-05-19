using OnForkHub.Core.Interfaces.GraphQL;

namespace OnForkHub.Application.GraphQL.Handlers;

public class GetAllCategoriesHandler(IUseCase<PaginationRequestDto, IEnumerable<Category>> useCase)
    : IGraphQLQueryHandler<PaginationRequestDto, IEnumerable<Category>>
{
    private readonly IUseCase<PaginationRequestDto, IEnumerable<Category>> _useCase = useCase;

    public async Task<RequestResult<IEnumerable<Category>>> HandleAsync(PaginationRequestDto input)
    {
        return await _useCase.ExecuteAsync(input);
    }
}
