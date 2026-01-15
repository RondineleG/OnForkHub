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
/// Endpoint for user registration.
/// </summary>
public sealed partial class RegisterEndpoint(
    ILogger<RegisterEndpoint> logger,
    ITokenService tokenService,
    IUseCase<UserRegisterRequestDto, UserEntity> registerUserUseCase) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/auth/register";

    private readonly ILogger<RegisterEndpoint> _logger = logger;

    private readonly ITokenService _tokenService = tokenService;

    private readonly IUseCase<UserRegisterRequestDto, UserEntity> _registerUserUseCase = registerUserUseCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                [AllowAnonymous]
                async ([FromBody] UserRegisterRequestDto request, CancellationToken cancellationToken) =>
                {
                    return await HandleRegisterAsync(request);
                }
            )
            .WithName("RegisterV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Authentication")
            .WithDescription("Registers a new user and returns JWT tokens")
            .WithSummary("User registration")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<AuthResponseDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Registration attempt for user: {Email}")]
    private partial void LogRegistrationAttempt(string email);

    private async Task<IResult> HandleRegisterAsync(UserRegisterRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        LogRegistrationAttempt(request.Email);

        var userResult = await _registerUserUseCase.ExecuteAsync(request);
        if (userResult.Status != EResultStatus.Success || userResult.Data is null)
        {
            return userResult.Status == EResultStatus.HasError
                ? Results.BadRequest(new { error = userResult.Message })
                : Results.Conflict(new { error = userResult.Message });
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

        return Results.Created($"/api/v{V1}/users/{user.Id}", response);
    }
}
