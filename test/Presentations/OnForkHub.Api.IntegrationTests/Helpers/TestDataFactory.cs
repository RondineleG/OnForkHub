namespace OnForkHub.Api.IntegrationTests.Helpers;

using OnForkHub.Application.Dtos.Category.Request;
using OnForkHub.Application.Dtos.User.Request;
using OnForkHub.Application.Dtos.Video.Request;

/// <summary>
/// Factory class for creating test data in integration tests.
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// Creates a unique email address for testing.
    /// </summary>
    /// <returns>Unique email address.</returns>
    public static string CreateUniqueEmail()
    {
        return $"test.{Guid.NewGuid():N}@example.com";
    }

    /// <summary>
    /// Creates a unique name for testing.
    /// </summary>
    /// <param name="prefix">Optional prefix.</param>
    /// <returns>Unique name.</returns>
    public static string CreateUniqueName(string prefix = "Test")
    {
        return $"{prefix} {Guid.NewGuid():N}";
    }

    /// <summary>
    /// Creates a valid user registration request.
    /// </summary>
    /// <param name="name">Optional user name.</param>
    /// <param name="email">Optional user email.</param>
    /// <param name="password">Optional password.</param>
    /// <returns>User registration request DTO.</returns>
    public static UserRegisterRequestDto CreateRegisterRequest(string? name = null, string? email = null, string password = "Test@123456")
    {
        return new UserRegisterRequestDto
        {
            Name = name ?? CreateUniqueName("User"),
            Email = email ?? CreateUniqueEmail(),
            Password = password,
        };
    }

    /// <summary>
    /// Creates a valid user login request.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns>User login request DTO.</returns>
    public static UserLoginRequestDto CreateLoginRequest(string email, string password)
    {
        return new UserLoginRequestDto { Email = email, Password = password };
    }

    /// <summary>
    /// Creates a valid category request.
    /// </summary>
    /// <param name="name">Optional category name.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>Category request DTO.</returns>
    public static CategoryRequestDto CreateCategoryRequest(string? name = null, string? description = null)
    {
        return new CategoryRequestDto { Name = name ?? CreateUniqueName("Category"), Description = description ?? "Test category description" };
    }

    /// <summary>
    /// Creates a valid video creation request.
    /// </summary>
    /// <param name="title">Optional video title.</param>
    /// <param name="userId">Optional user ID.</param>
    /// <param name="categoryIds">Optional category IDs.</param>
    /// <returns>Video creation request DTO.</returns>
    public static VideoCreateRequestDto CreateVideoRequest(string? title = null, string? userId = null, IReadOnlyList<long>? categoryIds = null)
    {
        return new VideoCreateRequestDto
        {
            Title = title ?? CreateUniqueName("Video"),
            Description = "This is a test video description for integration testing purposes.",
            Url = "https://example.com/video.mp4",
            UserId = userId ?? Guid.NewGuid().ToString(),
            CategoryIds = categoryIds ?? Array.Empty<long>(),
        };
    }

    /// <summary>
    /// Creates a valid video update request.
    /// </summary>
    /// <param name="id">Video ID.</param>
    /// <param name="title">Optional title.</param>
    /// <returns>Video update request DTO.</returns>
    public static VideoUpdateRequestDto CreateVideoUpdateRequest(string id, string? title = null)
    {
        return new VideoUpdateRequestDto
        {
            Id = id,
            Title = title ?? CreateUniqueName("UpdatedVideo"),
            Description = "Updated test video description.",
            Url = "https://example.com/updated-video.mp4",
        };
    }

    /// <summary>
    /// Creates a valid category update request.
    /// </summary>
    /// <param name="name">Optional category name.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>Category request DTO for update.</returns>
    public static CategoryRequestDto CreateCategoryUpdateRequest(string? name = null, string? description = null)
    {
        return new CategoryRequestDto
        {
            Name = name ?? CreateUniqueName("UpdatedCategory"),
            Description = description ?? "Updated test category description",
        };
    }
}
