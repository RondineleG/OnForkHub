namespace OnForkHub.Core.Interfaces.GraphQL;

public interface IGraphQLMutationHandler<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> HandleAsync(TRequest input);
}
