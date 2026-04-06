using OnForkHub.Core.Requests;

namespace OnForkHub.CrossCutting.GraphQL.Interfaces;

public interface IGraphQLMutationHandler<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> HandleAsync(TRequest input);
}
