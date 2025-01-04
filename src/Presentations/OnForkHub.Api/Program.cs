using OnForkHub.Core.Validations.Categories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v1",
            Title = "OnForkHub API - Version 1",
            Description = "API version 1 documentation",
        }
    );

    options.SwaggerDoc(
        "v2",
        new OpenApiInfo
        {
            Version = "v2",
            Title = "OnForkHub API - Version 2",
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
builder.Services.AddValidators(typeof(CategoryValidator).Assembly);
builder.Services.AddUseCases(typeof(GetAllCategoriesUseCase).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnForkHub API V1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "OnForkHub API V2");
    });
}

// app.UseHttpsRedirection(); //Only dev publish test
await app.RegisterWebApisAsync();
await app.RunAsync();
