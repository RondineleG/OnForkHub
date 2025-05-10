using GraphQL;
using GraphQL.SystemTextJson;

using OnForkHub.Core.Extensions;
using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

var builder = WebApplication.CreateBuilder(args);

var apiMode = builder.Configuration.GetValue<string>("AppSettings:ApiMode") ?? "All";

builder.Services.AddSwaggerServices();
builder.Services.AddRavenDbServices(builder.Configuration);
builder.Services.AddEntityFrameworkServices(builder.Configuration);
builder.Services.AddCustomServices();

var endpointManager = new GraphQLEndpointManager();
endpointManager.RegisterEndpoint(new HotChocolateEndpoint());
endpointManager.RegisterEndpoint(new GraphQLNetEndpoint());
builder.Services.AddSingleton(endpointManager);
endpointManager.ConfigureAll(builder.Services);

builder.Services.AddGraphQLServices();
builder.Services.AddGraphQLOperations(typeof(Program).Assembly, typeof(IGraphQLQuery).Assembly);
builder.Services.AddSingleton<IGraphQLTextSerializer, GraphQLSerializer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

Console.WriteLine($"API Mode: {apiMode}");
Console.WriteLine($"Number of registered endpoints: {endpointManager.Endpoints.Count}");

if (apiMode is "All" or "Rest")
{
    app.MapGroup("/api/v1/rest").MapRestEndpoints();
}

if (apiMode is "All" or "HotChocolate")
{
    app.MapGroup("/api/v1/graph/hc").MapHotChocolateEndpoints(endpointManager);
}

if (apiMode is "All" or "GraphQLNet")
{
    app.MapGroup("/api/v1/graph/gn").MapGraphQLNetEndpoints(endpointManager);
}

if (apiMode is "All" or "Rest")
{
    app.UseCustomSwagger();
}

await app.RunAsync();
