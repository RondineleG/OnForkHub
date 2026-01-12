namespace OnForkHub.Core.Interfaces.UseCases;

public interface IUseCase<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> ExecuteAsync(TRequest request);

    Task<RequestResult<TResponse>> ExecuteAsync(TRequest request, CancellationToken cancellationToken) => ExecuteAsync(request);
}
