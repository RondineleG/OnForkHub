namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Api.IntegrationTests.Infrastructure;
using OnForkHub.Core.Interfaces.Services;

using Xunit;

/// <summary>
/// Integration coverage for authenticated range-based video delivery.
/// </summary>
public sealed class VideoStreamingIntegrationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    /// <summary>
    /// A completed upload supports byte-range playback for its owner.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task StreamCompletedUpload_WithRange_ReturnsRequestedBytes()
    {
        using var client = factory.CreateClientWithAuth();
        var token = client.DefaultRequestHeaders.Authorization!.Parameter!;
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
        var userId = claims.First(c => c.Type is ClaimTypes.NameIdentifier or "nameid" or "sub").Value;
        byte[] videoBytes = [10, 20, 30, 40, 50, 60];

        await using var scope = factory.CreateScopeAsync();
        var uploads = scope.ServiceProvider.GetRequiredService<IVideoUploadService>();
        var initiated = await uploads.InitiateUploadAsync(
            "range-test.mp4", videoBytes.Length, "video/mp4", userId, 1);
        initiated.Data.Should().NotBeNull();
        var uploadId = Guid.Parse(initiated.Data!.Id.Split('/').Last());

        using (var content = new MemoryStream(videoBytes))
        {
            var sent = await uploads.UploadChunkAsync(uploadId, content, 0, 1);
            sent.Status.Should().Be(OnForkHub.Core.Enums.EResultStatus.Success);
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/videos/upload/{uploadId}/stream");
        request.Headers.Range = new RangeHeaderValue(1, 3);
        using var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.PartialContent);
        response.Content.Headers.ContentRange!.From.Should().Be(1);
        response.Content.Headers.ContentRange.To.Should().Be(3);
        (await response.Content.ReadAsByteArrayAsync()).Should().Equal(20, 30, 40);
    }
}
