namespace OnForkHub.Api.Endpoints.Base;

public abstract class BaseEndpoint<TEntity>
    where TEntity : BaseEntity
{
    protected static string GetVersionedRoute(int version)
    {
        var route = typeof(TEntity).Name.ToLowerInvariant();
        return $"/api/v{version}/{route}";
    }

    protected static ApiVersionSet CreateApiVersionSet(WebApplication app, int version)
    {
        return app.NewApiVersionSet().HasApiVersion(new ApiVersion(version)).ReportApiVersions().Build();
    }
}