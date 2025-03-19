namespace OnForkHub.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
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

            options.SwaggerDoc(
                "v3",
                new OpenApiInfo
                {
                    Version = "v3",
                    Title = "OnForkHub API - Version 3",
                    Description = "API version 3 documentation",
                }
            );
        });

        services
            .AddApiVersioning(options =>
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

        return services;
    }

    public static WebApplication UseCustomSwagger(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnForkHub API V1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "OnForkHub API V3");
                c.SwaggerEndpoint("/swagger/v3/swagger.json", "OnForkHub API V3");
            });
        }

        return app;
    }
}