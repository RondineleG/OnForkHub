using GraphQL;

using OnForkHub.Core.Extensions;
using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

var builder = WebApplication.CreateBuilder(args);
var apiMode = builder.Configuration.GetValue<string>("AppSettings:ApiMode") ?? "All";

builder.Services.AddSwaggerServices();
builder.Services.AddRavenDbServices(builder.Configuration);
builder.Services.AddEntityFrameworkServices(builder.Configuration);
builder.Services.AddCustomServices();

var endpointManager = new GraphQLServiceEndpointManager();
endpointManager.RegisterEndpoint(new HotChocolateEndpoint());
endpointManager.RegisterEndpoint(new GraphQLNetEndpoint());

builder.Services.AddSingleton(endpointManager);
endpointManager.ConfigureAll(builder.Services);

builder.Services.AddGraphQLServices();
builder.Services.AddGraphQLOperations(typeof(Program).Assembly, typeof(IGraphQLQuery).Assembly);
builder.Services.AddSingleton<IGraphQLTextSerializer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

Console.WriteLine($"API Mode: {apiMode}");
Console.WriteLine($"Number of registered endpoints: {endpointManager.Endpoints.Count}");

foreach (var endpoint in endpointManager.Endpoints)
{
    Console.WriteLine($"Found endpoint: {endpoint.GetType().Name} at path {endpoint.Path}");

    if (endpoint is HotChocolateEndpoint && (apiMode == "All" || apiMode == "HotChocolate"))
    {
        app.MapGraphQL(path: endpoint.Path);
        Console.WriteLine($"HotChocolate GraphQL endpoint registered at {endpoint.Path}");
    }
    else if (endpoint is GraphQLNetEndpoint && (apiMode == "All" || apiMode == "GraphQLNet"))
    {
        app.UseGraphQL(endpoint.Path);
        Console.WriteLine($"GraphQL.NET endpoint registered at {endpoint.Path}");
    }
}

if (apiMode is "All" or "Rest")
{
    app.UseCustomSwagger();
}

await app.UseEndpointsAsync();
await app.RunAsync();
