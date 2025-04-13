using OnForkHub.Api.Endpoints.GraphQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerServices();
builder.Services.AddRavenDbServices(builder.Configuration);
builder.Services.AddEntityFrameworkServices(builder.Configuration);
builder.Services.AddCustomServices();

builder.Services.AddGraphQLServer().AddQueries().AddMutations().AddFiltering().AddSorting();

var app = builder.Build();
app.UseCustomSwagger();
app.MapGraphQL("/graphql").WithName("OnForkHubGraphQL");
await app.UseEndpointsAsync().ConfigureAwait(false);
await app.RunAsync().ConfigureAwait(false);
