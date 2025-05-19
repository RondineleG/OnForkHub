// The .NET Foundation licenses this file to you under the MIT license.

using OnForkHub.Core.GraphQL;

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
