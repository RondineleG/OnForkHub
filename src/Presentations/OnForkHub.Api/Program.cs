var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerServices();
builder.Services.AddRavenDbServices(builder.Configuration);
builder.Services.AddEntityFrameworkServices(builder.Configuration);
builder.Services.AddCustomServices();
builder.Services.AddGraphQLServices();

var app = builder.Build();
app.UseCustomSwagger();
app.MapGraphQL();
await app.UseWebApisAsync();
await app.RunAsync();
