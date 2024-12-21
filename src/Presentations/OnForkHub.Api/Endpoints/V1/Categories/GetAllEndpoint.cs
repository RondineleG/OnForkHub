namespace OnForkHub.Api.Endpoints.V1.Categories;

public class GetAllEndpoint : IEndpoint
{
    private const string RouteV1 = "/api/v1/category";
    private const int V1 = 1;

    public void Register(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app);

        app.MapGet(RouteV1, HandleGetPersonsAsync)
            .WithName("GetAllCategoryV1")
            .Produces<IEnumerable<Category>>()
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags("Categories")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .CacheOutput(x => x.Expire(TimeSpan.FromMinutes(10)))
            .WithDescription("Returns all Categories")
            .WithSummary("List Categories");
    }

    private static ApiVersionSet CreateApiVersionSet(WebApplication app)
    {
        return app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();
    }

    private static async Task<IResult> HandleGetPersonsAsync([FromServices] ICategoryRepositoryEF personRepository)
    {
        try
        {
            var persons = await personRepository.GetAllAsync(1, 10);
            return Results.Ok(persons);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }
}