namespace OnForkHub.Api.Endpoints.Base;

public interface IBaseEndPoint
{
    Task<IResult> HandleAsync(params object[] args);
    ApiVersionSet CreateApiVersionSet(WebApplication app);
}
