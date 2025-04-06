using OnForkHub.Core.GraphQL;
using OnForkHub.CrossCutting.GraphQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerServices();
builder.Services.AddRavenDbServices(builder.Configuration);
builder.Services.AddEntityFrameworkServices(builder.Configuration);
builder.Services.AddCustomServices();
builder.Services.AddSingleton<IGraphQLConfigurator, HotChocolateConfigurator>();
builder.Services.AddGraphQLFromCrossCutting();

var app = builder.Build();
app.UseCustomSwagger();
app.MapGraphQL();
await app.UseWebApisAsync();
await app.RunAsync();
