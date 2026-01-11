using OnForkHub.Api.Middlewares;
using OnForkHub.Application.Extensions;
using OnForkHub.Core.Interfaces.GraphQL;
using OnForkHub.CrossCutting.Caching;
using OnForkHub.CrossCutting.Extensions;
using OnForkHub.CrossCutting.Middleware.RateLimiting;
using OnForkHub.CrossCutting.Middleware.ResponseCompression;
using OnForkHub.CrossCutting.Middleware.Security;

var builder = WebApplication.CreateBuilder(args);

var apiMode = builder.Configuration.GetValue<string>("AppSettings:ApiMode") ?? "All";

builder.Services.AddSwaggerServices();
builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddGraphQLFromCrossCutting();
builder.Services.AddGraphQLAdapters();
builder.Services.AddResponseCompressionServices();
builder.Services.AddCachingServices(builder.Configuration);
builder.Services.AddRateLimitingServices(builder.Configuration);

var app = builder.Build();

app.UseResponseCompressionMiddleware();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRateLimitingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseCustomSwagger();
}

// Global exception handler should be after other middleware to catch all exceptions
app.UseGlobalExceptionHandler();

app.UseMiddleware<ApiTypeDetectionMiddleware>();

if (apiMode is "Rest")
{
    app.MapGroup("/api/v1/rest").MapRestEndpoints();
}

if (apiMode is "HotChocolate")
{
    app.MapGroup("/api/v1/graph/hc").MapHotChocolateEndpoints(app.Services.GetRequiredService<GraphQLEndpointManager>());
}

if (apiMode is "GraphQLNet")
{
    app.MapGroup("/api/v1/graph/gn").MapGraphQLNetEndpoints(app.Services.GetRequiredService<GraphQLEndpointManager>());
}

await app.RunAsync();

/// <summary>
/// Partial class declaration for integration tests.
/// </summary>
public partial class Program { }
