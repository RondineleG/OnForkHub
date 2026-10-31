namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Core.Interfaces.Services;

using Xunit;

/// <summary>
/// Ensures an authenticated user cannot access another user's uploads.
/// </summary>
public sealed class VideoUploadAuthorizationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    /// <summary>
    /// Rejects a cross-account upload list request.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUploads_WithAnotherUserId_ReturnsForbidden()
    {
        using var client = factory.CreateClientWithAuth();
        var otherUserId = Guid.NewGuid();

        using var response = await client.GetAsync($"/api/v1/videos/uploads?userId={otherUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Rejects an attempt to initiate an upload for another account.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task InitiateUpload_WithAnotherUserId_ReturnsForbidden()
    {
        using var client = factory.CreateClientWithAuth();
        var request = new
        {
            fileName = "example.mp4",
            fileSize = 1024,
            contentType = "video/mp4",
            totalChunks = 1,
            userId = Guid.NewGuid().ToString(),
        };

        using var response = await client.PostAsJsonAsync("/api/v1/videos/upload/initiate", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Rejects reading another user's upload status.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetStatus_ForAnotherUsersUpload_ReturnsForbidden()
    {
        var uploadId = await CreateOtherUsersUploadAsync();
        using var client = factory.CreateClientWithAuth();

        using var response = await client.GetAsync($"/api/v1/videos/upload/{uploadId}/status");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Rejects modifying another user's upload through the chunk endpoint.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task UploadChunk_ForAnotherUsersUpload_ReturnsForbidden()
    {
        var uploadId = await CreateOtherUsersUploadAsync();
        using var client = factory.CreateClientWithAuth();
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent([1, 2, 3]), "chunk", "part.bin");
        form.Add(new StringContent("0"), "chunkIndex");
        form.Add(new StringContent("1"), "totalChunks");

        using var response = await client.PostAsync($"/api/v1/videos/upload/chunk/{uploadId}", form);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Rejects streaming content belonging to another user.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task StreamUpload_ForAnotherUsersUpload_ReturnsForbidden()
    {
        var uploadId = await CreateOtherUsersUploadAsync();
        using var client = factory.CreateClientWithAuth();

        using var response = await client.GetAsync($"/api/v1/videos/upload/{uploadId}/stream");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<Guid> CreateOtherUsersUploadAsync()
    {
        await using var scope = factory.CreateScopeAsync();
        var service = scope.ServiceProvider.GetRequiredService<IVideoUploadService>();
        var result = await service.InitiateUploadAsync(
            "other-user.mp4", 3, "video/mp4", Guid.NewGuid().ToString(), 1);

        result.Data.Should().NotBeNull();
        return Guid.Parse(result.Data!.Id.Split('/').Last());
    }
}
