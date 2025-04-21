using OnForkHub.CrossCutting.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerServices();
builder.Services.AddRavenDbServices(builder.Configuration);
builder.Services.AddEntityFrameworkServices(builder.Configuration);
builder.Services.AddCustomServices();
builder.Services.AddGraphQLFromCrossCutting();

var app = builder.Build();
app.UseCustomSwagger();
await app.UseEndpointsAsync();
app.MapGraphQL("/graphql").WithName("OnForkHubGraphQL");
await app.RunAsync();
