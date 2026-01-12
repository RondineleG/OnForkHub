namespace OnForkHub.CrossCutting.Middleware.RateLimiting;

using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring rate limiting services.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Policy name for the default rate limiter.
    /// </summary>
    public const string DefaultPolicy = "default";

    /// <summary>
    /// Policy name for authenticated users rate limiter.
    /// </summary>
    public const string AuthenticatedPolicy = "authenticated";

    /// <summary>
    /// Policy name for anonymous users rate limiter.
    /// </summary>
    public const string AnonymousPolicy = "anonymous";

    /// <summary>
    /// Adds rate limiting services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new RateLimitingOptions();
        configuration.GetSection(RateLimitingOptions.SectionName).Bind(options);

        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        if (!options.Enabled)
        {
            return services;
        }

        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiterOptions.AddFixedWindowLimiter(
                DefaultPolicy,
                windowOptions =>
                {
                    windowOptions.PermitLimit = options.PermitLimit;
                    windowOptions.Window = TimeSpan.FromSeconds(options.WindowSeconds);
                    windowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    windowOptions.QueueLimit = options.QueueLimit;
                }
            );

            limiterOptions.AddFixedWindowLimiter(
                AuthenticatedPolicy,
                windowOptions =>
                {
                    windowOptions.PermitLimit = options.AuthenticatedPermitLimit;
                    windowOptions.Window = TimeSpan.FromSeconds(options.WindowSeconds);
                    windowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    windowOptions.QueueLimit = options.QueueLimit * 2;
                }
            );

            limiterOptions.AddFixedWindowLimiter(
                AnonymousPolicy,
                windowOptions =>
                {
                    windowOptions.PermitLimit = options.AnonymousPermitLimit;
                    windowOptions.Window = TimeSpan.FromSeconds(options.WindowSeconds);
                    windowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    windowOptions.QueueLimit = options.QueueLimit / 2;
                }
            );

            limiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                double? retryAfterSeconds = null;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retryAfterSeconds = retryAfter.TotalSeconds;
                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString("0", CultureInfo.InvariantCulture);
                }

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        error = "Too many requests",
                        message = "Rate limit exceeded. Please try again later.",
                        retryAfter = retryAfterSeconds,
                    },
                    cancellationToken
                );
            };
        });

        return services;
    }

    /// <summary>
    /// Uses rate limiting middleware in the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The modified application builder.</returns>
    public static IApplicationBuilder UseRateLimitingMiddleware(this IApplicationBuilder app)
    {
        app.UseRateLimiter();
        return app;
    }
}
