using Microsoft.AspNetCore.Builder;

namespace OnForkHub.Core.Interfaces.Configuration;

public interface IEndpointAsync
{
    Task<RequestResult> RegisterAsync(WebApplication app);
}
