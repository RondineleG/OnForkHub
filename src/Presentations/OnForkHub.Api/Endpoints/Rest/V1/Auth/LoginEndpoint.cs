namespace OnForkHub.Api.Endpoints.Rest.V1.Auth;

using Microsoft.AspNetCore.Authorization;
using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.Dtos.User.Response;
using OnForkHub.Core.Interfaces.Configuration;
using OnForkHub.CrossCutting.Authentication;

/// <summary>
/// Endpoint for user login.
/// </summary>
public sealed partial class LoginEndpoint(ILogger<LoginEndpoint> logger, ITokenService tokenService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/auth/login";

    private readonly ILogger<LoginEndpoint> _logger = logger;

    private readonly ITokenService _tokenService = tokenService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                [AllowAnonymous]
                async ([FromBody] UserLoginRequestDto request, CancellationToken cancellationToken) =>
                {
                    return await HandleLoginAsync(request);
                }
            )
            .WithName("LoginV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Authentication")
            .WithDescription("Authenticates a user and returns JWT tokens")
            .WithSummary("User login")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Login attempt for user: {Email}")]
    private partial void LogLoginAttempt(string email);

    private Task<IResult> HandleLoginAsync(UserLoginRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        LogLoginAttempt(request.Email);

        var tokens = _tokenService.GenerateTokens(
            userId: Guid.NewGuid().ToString(),
            userName: request.Email,
            roles: [CrossCutting.Authorization.Roles.User]
        );

        var response = new AuthResponseDto
        {
            User = new UserResponseDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Email.Split('@')[0],
                Email = request.Email,
                Roles = [CrossCutting.Authorization.Roles.User],
                CreatedAt = DateTime.UtcNow,
            },
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiration = tokens.AccessTokenExpiration,
            RefreshTokenExpiration = tokens.RefreshTokenExpiration,
        };

        return Task.FromResult(Results.Ok(response));
    }
}
