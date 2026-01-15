namespace OnForkHub.CrossCutting.Middleware.ResponseCompression;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

using System.IO.Compression;
using System.Runtime.InteropServices;

/// <summary>
/// Extensions for adding response compression services to the dependency injection container.
/// </summary>
public static class ResponseCompressionExtensions
{
    /// <summary>
    /// Additional MIME types to include in response compression.
    /// </summary>
    private static readonly string[] AdditionalMimeTypes =
    [
        "application/graphql-response+json",
        "application/javascript",
        "application/json",
        "application/ld+json",
        "application/xml",
        "image/svg+xml",
        "text/css",
        "text/html",
        "text/plain",
        "text/xml",
    ];

    /// <summary>
    /// Adds response compression services to the dependency injection container.
    /// Supports Gzip and Brotli compression algorithms.
    /// </summary>
    /// <param name="services">The service collection to add compression services to.</param>
    /// <returns>The modified service collection for chaining.</returns>
    public static IServiceCollection AddResponseCompressionServices(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();

            if (
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            )
            {
                options.Providers.Add<BrotliCompressionProvider>();
            }

            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(AdditionalMimeTypes);
            options.EnableForHttps = true;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        return services;
    }

    /// <summary>
    /// Uses response compression middleware in the HTTP request pipeline.
    /// Should be added early in the pipeline, preferably before other middlewares.
    /// </summary>
    /// <param name="app">The application builder to add compression middleware to.</param>
    /// <returns>The modified application builder for chaining.</returns>
    public static IApplicationBuilder UseResponseCompressionMiddleware(this IApplicationBuilder app)
    {
        app.UseResponseCompression();
        return app;
    }
}
