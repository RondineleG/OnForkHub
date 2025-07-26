namespace OnForkHub.Api.Configuration;

public interface IEndpoint
{
    void Register(WebApplication app);
}
