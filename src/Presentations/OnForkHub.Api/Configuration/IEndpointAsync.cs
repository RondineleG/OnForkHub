namespace OnForkHub.Api.Configuration;

public interface IEndpointAsync
{
    Task<RequestResult> RegisterAsync(WebApplication app);
}
