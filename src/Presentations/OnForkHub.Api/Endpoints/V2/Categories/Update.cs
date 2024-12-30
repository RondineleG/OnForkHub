using OnForkHub.Application.Dtos.Category.Request;

namespace OnForkHub.Api.Endpoints.V2.Categories;

public class Update(ILogger<Update> logger, IUseCase<CategoryUpdateRequestDto, Category> useCase) : BaseEndpoint<Category>, IEndpointAsync
{
    private const int V2 = 2;
    private static readonly string Route = $"{GetVersionedRoute(V2)}/{{id}}";
    private readonly ILogger<Update> _logger = logger;
    private readonly IUseCase<CategoryUpdateRequestDto, Category> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V2);

        ConfigureEndpoint(
                app.MapPut(
                    Route,
                    async ([FromRoute] long id, [FromBody] CategoryRequestDto request, CancellationToken cancellationToken) =>
                    {
                        var updateRequest = new CategoryUpdateRequestDto { Id = id, Category = request };
                        return await HandleUseCase(_useCase, _logger, updateRequest);
                    }
                )
            )
            .WithName("UpdateCategoryV2")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V2)
            .WithDescription("Updates an existing category")
            .WithSummary("Update category")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V2}" })
            .Produces<RequestResult<Category>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}