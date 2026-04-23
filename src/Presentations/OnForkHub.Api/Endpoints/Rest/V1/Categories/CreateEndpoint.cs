using OnForkHub.Application.Dtos.Category.Request;
using OnForkHub.Core.Responses.Categories;
using OnForkHub.CrossCutting.Interfaces;

namespace OnForkHub.Api.Endpoints.Rest.V1.Categories;

public class CreateEndpoint(ILogger<CreateEndpoint> logger, IUseCase<CategoryRequestDto, Category> useCase) : BaseEndPoint<Category>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1);

    private readonly ILogger<CreateEndpoint> _logger = logger;

    private readonly IUseCase<CategoryRequestDto, Category> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapPost(
                    Route,
                    async ([FromBody] CategoryRequestDto request, CancellationToken cancellationToken) =>
                    {
                        return await HandleUseCaseAsync(
                            _useCase,
                            _logger,
                            request,
                            result =>
                            {
                                var response = new
                                {
                                    data = result.Data != null ? CategoryResponse.FromEntity(result.Data) : null,
                                    message = result.Message,
                                    date = result.Date,
                                    id = result.Id,
                                };

                                return Results.Created($"{Route}/{result.Data?.Id}", response);
                            },
                            "Failed to create category",
                            cancellationToken
                        );
                    }
                )
            )
            .WithName("CreateCategoryV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Creates a new category")
            .WithSummary("Create category")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<CategoryResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
