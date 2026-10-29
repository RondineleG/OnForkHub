namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using OnForkHub.Api.IntegrationTests.Infrastructure;

using Xunit;

/// <summary>
/// Ensures an authenticated user cannot access another user's uploads.
/// </summary>
public sealed class VideoUploadAuthorizationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    /// <summary>
    /// Rejects a cross-account upload list request.
    /// </summary>
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
}
