using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Interfaces.UseCases;

public interface IUseCase<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> ExecuteAsync(TRequest request);
}
