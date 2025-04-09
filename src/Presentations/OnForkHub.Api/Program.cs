using OnForkHub.Application.GraphQL.Mutations.Categories;
using OnForkHub.Application.GraphQL.Queries.Categories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerServices();
builder.Services.AddRavenDbServices(builder.Configuration);
builder.Services.AddEntityFrameworkServices(builder.Configuration);
builder.Services.AddCustomServices();

builder
    .Services.AddGraphQLServer()
    .AddQueryType()
    .AddTypeExtension<GetAllCategoryQuery>()
    .AddMutationType()
    .AddTypeExtension<CreateCategoryMutation>()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();
app.UseCustomSwagger();
await app.UseEndpoinAsync();
app.MapGraphQL("/graphql").WithName("OnForkHubGraphQL");
await app.RunAsync();
