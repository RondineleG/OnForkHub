using OnForkHub.Api.Middlewares;
using OnForkHub.Application.Extensions;
using OnForkHub.Core.Interfaces.GraphQL;
using OnForkHub.CrossCutting.Extensions;

var builder = WebApplication.CreateBuilder(args);

var apiMode = builder.Configuration.GetValue<string>("AppSettings:ApiMode") ?? "All";

builder.Services.AddSwaggerServices();
builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddGraphQLFromCrossCutting();
builder.Services.AddGraphQLAdapters();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCustomSwagger();
}

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
