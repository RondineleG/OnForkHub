namespace OnForkHub.Api.Endpoints.Rest.V1.Categories;

public class Create(ILogger<Create> logger, IUseCase<CategoryRequestDto, Category> useCase) : BaseEndPoint<Category>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1);

    private readonly ILogger<Create> _logger = logger;

    private readonly IUseCase<CategoryRequestDto, Category> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapPost(
                    Route,
                    async ([FromBody] CategoryRequestDto request, CancellationToken cancellationToken) =>
                    {
                        return await HandleCreateUseCase(_useCase, _logger, request);
                    }
                )
            )
            .WithName("CreateCategoryV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Creates a new category")
            .WithSummary("Create category")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<Category>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return Task.FromResult(RequestResult.Success());
    }
}
