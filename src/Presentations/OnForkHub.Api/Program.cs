using OnForkHub.Core.Extensions;
using OnForkHub.CrossCutting.Extensions;
using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerServices();
builder.Services.AddRavenDbServices(builder.Configuration);
builder.Services.AddEntityFrameworkServices(builder.Configuration);
builder.Services.AddCustomServices();
builder.Services.AddGraphQLFromCrossCutting();
builder.Services.AddGraphQLServices();

builder.Services.AddGraphQLOperations(typeof(Program).Assembly, typeof(IGraphQLQuery).Assembly);
var app = builder.Build();
app.UseCustomSwagger();
await app.UseEndpointsAsync();
var endpointManager = app.Services.GetRequiredService<GraphQLEndpointManager>();

foreach (var endpoint in endpointManager.Endpoints)
{
    if (endpoint is HotChocolateEndpoint)
    {
        app.UseGraphQL(endpoint.Path);
    }
    else if (endpoint is GraphQLNetEndpoint)
    {
        app.UseGraphQL(endpoint.Path);
    }
}

await app.RunAsync();
