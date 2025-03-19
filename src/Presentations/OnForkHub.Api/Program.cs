var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerServices();
builder.Services.AddRavenDbServices(builder.Configuration);
builder.Services.AddEntityFrameworkServices(builder.Configuration);
builder.Services.AddCustomServices();

var app = builder.Build();
app.UseCustomSwagger();
await app.UseWebApisAsync();
await app.RunAsync();