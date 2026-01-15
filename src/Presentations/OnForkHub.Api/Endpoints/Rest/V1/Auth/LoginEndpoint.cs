namespace OnForkHub.Api.Endpoints.Rest.V1.Auth;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.Dtos.User.Response;
using OnForkHub.Application.UseCases.Users;
using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Configuration;
using OnForkHub.CrossCutting.Authentication;

using UserEntity = OnForkHub.Core.Entities.User;

/// <summary>
/// Endpoint for user login.
/// </summary>
public sealed partial class LoginEndpoint(
    ILogger<LoginEndpoint> logger,
    ITokenService tokenService,
    IUseCase<UserLoginRequestDto, UserEntity> loginUserUseCase
) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/auth/login";

    private readonly ILogger<LoginEndpoint> _logger = logger;

    private readonly ITokenService _tokenService = tokenService;

    private readonly IUseCase<UserLoginRequestDto, UserEntity> _loginUserUseCase = loginUserUseCase;

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

    private async Task<IResult> HandleLoginAsync(UserLoginRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        LogLoginAttempt(request.Email);

        var userResult = await _loginUserUseCase.ExecuteAsync(request);
        if (userResult.Status != EResultStatus.Success || userResult.Data is null)
        {
            return Results.Unauthorized();
        }

        var user = userResult.Data;
        var tokens = _tokenService.GenerateTokens(
            userId: user.Id.ToString(),
            userName: user.Name.Value,
            roles: [CrossCutting.Authorization.Roles.User]
        );

        var response = new AuthResponseDto
        {
            User = UserResponseDto.FromUser(user, [CrossCutting.Authorization.Roles.User]),
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiration = tokens.AccessTokenExpiration,
            RefreshTokenExpiration = tokens.RefreshTokenExpiration,
        };

        return Results.Ok(response);
    }
}
