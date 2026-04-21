using Microsoft.AspNetCore.Builder;
using OnForkHub.Core.Requests;

namespace OnForkHub.CrossCutting.Interfaces;

public interface IEndpointAsync
{
    Task<RequestResult> RegisterAsync(WebApplication app);
}
