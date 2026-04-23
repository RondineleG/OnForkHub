namespace OnForkHub.CrossCutting.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddTracingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jaegerHost = configuration["Jaeger:Host"] ?? "localhost";
        var jaegerPort = configuration.GetValue<int>("Jaeger:Port", 4317); // Default OTLP gRPC port

        services
            .AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OnForkHub.Api"))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri($"http://{jaegerHost}:{jaegerPort}");
                    });
            });

        return services;
    }
}
