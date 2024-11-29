using Asp.Versioning;

using OnForkHub.Api.Configuration;
using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Repositories;

namespace OnForkHub.Api.Endpoints.V2.Categories;

public class GetAllCategoriesEndpoint : IEndpointAsync
{
    private const string RouteV2 = "/api/v2/person";
    private const int V2 = 2;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        try
        {
            var apiVersionSet = CreateApiVersionSet(app);

            app.MapGet(RouteV2, async ([FromServices] ICategoryRepositoryEF personRepository) =>
            {
                try
                {
                    var persons = await personRepository.GetAsync(1, 10);
                    return (persons?.Data is null)
                        ? TypedResults.Ok(RequestResult<IEnumerable<Category>>.WithNoContent())
                        : TypedResults.Ok(RequestResult<IEnumerable<Category>>.Success(persons.Data));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(RequestResult.WithError(ex));
                }
            })
               .WithName("GetCategoriesV2")
               .Produces<IEnumerable<Category>>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status500InternalServerError)
               .WithTags("Pessoas")
               .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V2}" })
               .WithApiVersionSet(apiVersionSet)
               .MapToApiVersion(V2)
               .CacheOutput(x => x.Expire(TimeSpan.FromMinutes(10)))
               .WithDescription("Returns all categories")
               .WithSummary("List categories");

            return Task.FromResult(RequestResult.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(RequestResult.WithError(ex));
        }
    }

    private static ApiVersionSet CreateApiVersionSet(WebApplication app)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(V2))
            .ReportApiVersions()
            .Build();
    }
}
