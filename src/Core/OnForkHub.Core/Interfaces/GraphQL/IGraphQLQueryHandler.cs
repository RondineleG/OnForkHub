namespace OnForkHub.Core.Interfaces.GraphQL;

public interface IGraphQLQueryHandler<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> HandleAsync(TRequest input);
}
