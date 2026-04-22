using OnForkHub.Api.Hubs;
using OnForkHub.Api.Middlewares;
using OnForkHub.Application.Extensions;
using OnForkHub.CrossCutting.Authentication;
using OnForkHub.CrossCutting.Caching;
using OnForkHub.CrossCutting.Extensions;
using OnForkHub.CrossCutting.GraphQL.Interfaces;
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
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "DefaultPolicy",
        policy =>
        {
            var allowedOrigins =
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                [
                    "http://localhost:5000",
                    "https://localhost:5001",
                    "http://localhost:3000",
                ];
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    );
});

builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseCors("DefaultPolicy");
app.UseAuthentication();
app.UseAuthorization();

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

if (apiMode is "Rest" or "All")
{
    await app.MapRegisteredEndpointsAsync();
}

if (apiMode is "HotChocolate" or "All")
{
    app.MapGroup("/api/v1/graph/hc").MapHotChocolateEndpoints(app.Services.GetRequiredService<GraphQLEndpointManager>());
}

if (apiMode is "GraphQLNet" or "All")
{
    app.MapGroup("/api/v1/graph/gn").MapGraphQLNetEndpoints(app.Services.GetRequiredService<GraphQLEndpointManager>());
}

app.MapHub<NotificationHub>("/hubs/notifications");

await app.RunAsync();
