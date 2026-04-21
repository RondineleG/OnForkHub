namespace OnForkHub.Api.Endpoints.Rest.V1.Users;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.Dtos.User.Response;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.ValueObjects;

/// <summary>
/// Endpoint for user profile management.
/// </summary>
public sealed class UserProfileEndpoint(IUserService userService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string GetProfileRoute = $"/api/v{V1}/users/profile";

    private static readonly string UpdateProfileRoute = $"/api/v{V1}/users/profile";

    private readonly IUserService _userService = userService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapGet(
                GetProfileRoute,
                [Authorize]
                async (ClaimsPrincipal user, CancellationToken cancellationToken) =>
                {
                    return await HandleGetProfileAsync(user, cancellationToken);
                }
            )
            .WithName("GetUserProfileV1")
            .WithApiVersionSet(apiVersionSet)
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPut(
                UpdateProfileRoute,
                [Authorize]
                async ([FromBody] UpdateUserProfileRequestDto request, ClaimsPrincipal user, CancellationToken cancellationToken) =>
                {
                    return await HandleUpdateProfileAsync(request, user, cancellationToken);
                }
            )
            .WithName("UpdateUserProfileV1")
            .WithApiVersionSet(apiVersionSet)
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return Task.FromResult(RequestResult.Success());
    }

    private async Task<IResult> HandleGetProfileAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        Id id = userId;
        var result = await _userService.GetByIdAsync(id);

        if (result.Status == EResultStatus.HasError)
        {
            return Results.NotFound();
        }

        if (result.Status != EResultStatus.Success || result.Data is null)
        {
            return Results.BadRequest(new { Message = result.Message });
        }

        var userEntity = result.Data;
        var response = new UserResponseDto
        {
            Id = userEntity.Id.ToString(),
            Name = userEntity.Name.Value,
            Email = userEntity.Email.Value,
            AvatarUrl = userEntity.AvatarUrl,
        };

        return Results.Ok(response);
    }

    private async Task<IResult> HandleUpdateProfileAsync(
        UpdateUserProfileRequestDto request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        if (request is null)
        {
            return Results.BadRequest(new { Message = "Request body is required" });
        }

        var result = await _userService.UpdateProfileAsync(userId, request.Name, request.Email);

        if (result.Status == EResultStatus.HasError && result.Message?.Contains("not found") == true)
        {
            return Results.NotFound();
        }

        if (result.Status == EResultStatus.HasValidation)
        {
            return Results.BadRequest(new { Validations = result.Validations });
        }

        if (result.Status != EResultStatus.Success || result.Data is null)
        {
            return Results.BadRequest(new { Message = result.Message });
        }

        var userEntity = result.Data;
        var response = new UserResponseDto
        {
            Id = userEntity.Id.ToString(),
            Name = userEntity.Name.Value,
            Email = userEntity.Email.Value,
            AvatarUrl = userEntity.AvatarUrl,
        };

        return Results.Ok(response);
    }
}
