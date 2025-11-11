namespace OnForkHub.Api.Middlewares;

public class ApiTypeDetectionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var requestPath = httpContext.Request.Path.Value;
        httpContext.Items["ApiMode"] = requestPath switch
        {
            var path when path is not null && path.StartsWith("/api/v1/rest", StringComparison.Ordinal) => "Rest",
            var path when path is not null && path.StartsWith("/api/v1/graph/hc", StringComparison.Ordinal) => "HotChocolate",
            var path when path is not null && path.StartsWith("/api/v1/graph/gn", StringComparison.Ordinal) => "GraphQLNet",
            _ => string.Empty,
        };
        await _next(httpContext);
    }
}
