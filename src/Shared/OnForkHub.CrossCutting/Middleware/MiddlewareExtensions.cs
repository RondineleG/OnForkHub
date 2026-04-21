namespace OnForkHub.CrossCutting.Extensions;

/// <summary>
/// Extension methods for middleware registration and configuration.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds global exception handling middleware to the HTTP request pipeline.
    /// Should be called early in the middleware pipeline to catch all exceptions.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<OnForkHub.CrossCutting.Middleware.GlobalExceptionHandlerMiddleware>();
    }
}
