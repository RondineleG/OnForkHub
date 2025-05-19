// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Interfaces.UseCases;

public interface IUseCase<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> ExecuteAsync(TRequest request);
}
