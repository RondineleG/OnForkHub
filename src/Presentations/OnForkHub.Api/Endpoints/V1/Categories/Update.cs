using OnForkHub.Application.Dtos.Category.Request;

namespace OnForkHub.Api.Endpoints.V1.Categories;

public class Update(ILogger<Create> logger, IUseCase<CategoryRequestDto, Category> useCase) : BaseEndpoint<Category>, IEndpointAsync
{
    private const int V3 = 3;

    private static readonly string Route = GetVersionedRoute(V3);

    private readonly ILogger<Create> _logger = logger;

    private readonly IUseCase<CategoryRequestDto, Category> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V3);

        ConfigureEndpoint(
                app.MapPost(
                    Route,
                    async ([FromBody] CategoryRequestDto request, CancellationToken cancellationToken) =>
                    {
                        return await HandleCreateUseCase(_useCase, _logger, request);
                    }
                )
            )
            .WithName("CreateCategoryV3")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V3)
            .WithDescription("Creates a new category")
            .WithSummary("Create category")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V3}" })
            .Produces<RequestResult<Category>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return Task.FromResult(RequestResult.Success());
    }
}
