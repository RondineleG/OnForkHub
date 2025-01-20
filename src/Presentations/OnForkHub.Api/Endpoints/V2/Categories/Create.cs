using OnForkHub.Application.Dtos.Category.Request;

namespace OnForkHub.Api.Endpoints.V2.Categories;

public class Create(ILogger<Create> logger, IUseCase<CategoryRequestDto, Category> useCase) : BaseEndpoint<Category>, IEndpointAsync
{
    private const int V2 = 2;

    private static readonly string Route = GetVersionedRoute(V2);

    private readonly ILogger<Create> _logger = logger;

    private readonly IUseCase<CategoryRequestDto, Category> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V2);

        ConfigureEndpoint(
                app.MapPost(
                    Route,
                    async ([FromBody] CategoryRequestDto request, CancellationToken cancellationToken) =>
                    {
                        return await HandleCreateUseCase(_useCase, _logger, request);
                    }
                )
            )
            .WithName("CreateCategoryV2")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V2)
            .WithDescription("Creates a new category")
            .WithSummary("Create category")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V2}" })
            .Produces<RequestResult<Category>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}