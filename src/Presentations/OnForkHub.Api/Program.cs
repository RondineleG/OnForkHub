using OnForkHub.Persistence.Contexts;
using OnForkHub.Persistence.Contexts.Base;
using OnForkHub.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v1",
            Title = "Minimal API - Version 1",
            Description = "API version 1 documentation",
        }
    );

    options.SwaggerDoc(
        "v2",
        new OpenApiInfo
        {
            Version = "v2",
            Title = "Minimal API - Version 2",
            Description = "API version 2 documentation",
        }
    );
});

builder
    .Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddWebApi(typeof(Program));
var configurationBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appSettings.json", true, true);
IConfiguration configuration = configurationBuilder.Build();
var connectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EntityFrameworkDataContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IEntityFrameworkDataContext, EntityFrameworkDataContext>();
builder.Services.AddScoped<ICategoryRepositoryEF, CategoryRepositoryEF>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API V1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Minimal API V2");
    });
}

app.UseHttpsRedirection();
await app.RegisterWebApisAsync();
await app.RunAsync();
