using OnForkHub.Api.Endpoints.Base;
using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Repositories;

namespace OnForkHub.Api.Endpoints.V2.Categories;

public class GetAllCategoriesEndpoint : BaseEndpoint<Category>, IEndpointAsync
{
    private const int V2 = 2;

    private static readonly string Route = GetVersionedRoute(V2);

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        try
        {
            var apiVersionSet = CreateApiVersionSet(app, V2);

            app.MapGet(
                    Route,
                    async ([FromServices] ICategoryRepositoryEF personRepository) =>
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
                    }
                )
                .WithName("GetCategoriesV2")
                .Produces<IEnumerable<Category>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithTags("Categories")
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
}
