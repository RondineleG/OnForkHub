namespace OnForkHub.Api.Endpoints.Rest.V1.Categories;

using OnForkHub.Application.Dtos.Category.Request;
using OnForkHub.CrossCutting.Interfaces;

public class UpdateEndpoint(ILogger<UpdateEndpoint> logger, IUseCase<CategoryUpdateRequest, Category> useCase)
    : BaseEndPoint<Category>,
        IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id:long}";

    private readonly ILogger<UpdateEndpoint> _logger = logger;

    private readonly IUseCase<CategoryUpdateRequest, Category> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapPut(
                    Route,
                    async ([FromRoute] long id, [FromBody] CategoryRequest request, CancellationToken cancellationToken) =>
                    {
                        var updateRequest = new CategoryUpdateRequest { Id = id, Category = request };
                        return await HandleUseCase(_useCase, _logger, updateRequest, cancellationToken);
                    }
                )
            )
            .WithName("UpdateCategoryV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Updates an existing category")
            .WithSummary("Update category")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<Category>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
