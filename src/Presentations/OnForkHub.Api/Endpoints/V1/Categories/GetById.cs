using OnForkHub.Application.Dtos.Category.Request;

namespace OnForkHub.Api.Endpoints.V1.Categories;

public class GetById(ILogger<Create> logger, IUseCase<CategoryRequestDto, Category> useCase) : BaseEndpoint<Category>, IEndpointAsync
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
            .WithName("GetByIdCategory")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Get category by id")
            .WithSummary("Get category")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<Category>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return Task.FromResult(RequestResult.Success());
    }
}
