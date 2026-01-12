namespace OnForkHub.Api.Endpoints.Rest.V1.Auth;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Core.Interfaces.Configuration;
using OnForkHub.CrossCutting.Authentication;

/// <summary>
/// Endpoint for refreshing JWT tokens.
/// </summary>
public sealed partial class RefreshTokenEndpoint(ILogger<RefreshTokenEndpoint> logger, ITokenService tokenService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/auth/refresh";

    private readonly ILogger<RefreshTokenEndpoint> _logger = logger;

    private readonly ITokenService _tokenService = tokenService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                [AllowAnonymous]
                async ([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken) =>
                {
                    return await HandleRefreshAsync(request);
                }
            )
            .WithName("RefreshTokenV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Authentication")
            .WithDescription("Refreshes JWT tokens using a valid refresh token")
            .WithSummary("Refresh tokens")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Token refresh attempt")]
    private partial void LogTokenRefreshAttempt();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Token refresh failed - invalid tokens")]
    private partial void LogTokenRefreshFailed();

    private Task<IResult> HandleRefreshAsync(RefreshTokenRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        LogTokenRefreshAttempt();

        var tokens = _tokenService.RefreshToken(request.AccessToken, request.RefreshToken);

        if (tokens is null)
        {
            LogTokenRefreshFailed();
            return Task.FromResult(Results.Unauthorized());
        }

        var response = new
        {
            accessToken = tokens.AccessToken,
            refreshToken = tokens.RefreshToken,
            accessTokenExpiration = tokens.AccessTokenExpiration,
            refreshTokenExpiration = tokens.RefreshTokenExpiration,
        };

        return Task.FromResult(Results.Ok(response));
    }
}
