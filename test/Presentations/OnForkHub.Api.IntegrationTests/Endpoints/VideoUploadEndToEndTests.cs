namespace OnForkHub.Api.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using OnForkHub.Api.IntegrationTests.Infrastructure;

using Xunit;

/// <summary>
/// End-to-end coverage of the core product flow against real services:
/// register -> login -> initiate upload -> chunks -> asynchronous processing ->
/// publish -> feed -> stream. Processing runs in passthrough mode on hosts
/// without ffmpeg, which still exercises the full state machine and queue.
/// </summary>
public class VideoUploadEndToEndTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly byte[] Mp4Payload =
    {
        0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32, 0x00, 0x00, 0x00, 0x00,
        0x6D, 0x70, 0x34, 0x32, 0x69, 0x73, 0x6F, 0x6D, 0x00, 0x00, 0x00, 0x00, 0x66, 0x72, 0x65, 0x65,
    };

    private readonly HttpClient _anonymousClient = factory.CreateClient();

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RegisterLoginUploadProcessPublishFeedAndStream_WorksEndToEnd()
    {
        var email = $"e2e-{Guid.NewGuid():N}@example.com";
        const string password = "Str0ng!Password";

        var registerResponse = await _anonymousClient.PostAsJsonAsync("/api/v1/auth/register", new { Name = "E2E User", Email = email, Password = password });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResponse = await _anonymousClient.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = auth.GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var categoryId = await EnsureCategoryAsync(client);

        var initiateResponse = await client.PostAsJsonAsync(
            "/api/v1/videos/upload/initiate",
            new
            {
                FileName = "e2e-video.mp4",
                FileSize = Mp4Payload.Length,
                ContentType = "video/mp4",
                TotalChunks = 2,
                UserId = "ignored-server-uses-jwt",
            }
        );
        initiateResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var initiated = await initiateResponse.Content.ReadFromJsonAsync<JsonElement>();
        var uploadId = initiated.GetProperty("id").GetString();
        uploadId.Should().NotBeNullOrWhiteSpace();

        var half = Mp4Payload.Length / 2;
        await UploadChunkAsync(client, uploadId!, Mp4Payload[..half], 0, 2);
        await UploadChunkAsync(client, uploadId!, Mp4Payload[half..], 1, 2);

        var finalStatus = await WaitForProcessingAsync(client, uploadId!, TimeSpan.FromSeconds(45));
        finalStatus.Should().Be("Completed", "the background worker must process the upload asynchronously");

        var publishResponse = await client.PostAsJsonAsync(
            $"/api/v1/videos/upload/{uploadId}/publish",
            new { Title = "E2E Video", Description = "Published end to end", CategoryId = categoryId }
        );
        var publishError = await publishResponse.Content.ReadAsStringAsync();
        publishResponse.StatusCode.Should().Be(HttpStatusCode.Created, $"publish must succeed: {publishError}");
        var published = await publishResponse.Content.ReadFromJsonAsync<JsonElement>();
        var videoId = published.GetProperty("id").GetString();
        videoId.Should().NotBeNullOrWhiteSpace();

        var feedResponse = await client.GetAsync("/api/v1/rest/videos");
        feedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var feedBody = await feedResponse.Content.ReadAsStringAsync();
        feedBody.Should().Contain(videoId!, "the published video must appear in the feed");

        var streamResponse = await client.GetAsync($"/api/v1/videos/upload/{uploadId}/stream");
        streamResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var streamed = await streamResponse.Content.ReadAsByteArrayAsync();
        streamed.Should().BeEquivalentTo(Mp4Payload, "the streamed bytes must be exactly the uploaded file");
    }

    private static async Task<string> EnsureCategoryAsync(HttpClient client)
    {
        var listResponse = await client.GetAsync("/api/v1/rest/categories");
        if (listResponse.StatusCode == HttpStatusCode.OK)
        {
            var body = await listResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var id = TryFindFirstCategoryId(doc.RootElement);
            if (!string.IsNullOrEmpty(id))
            {
                return id!;
            }
        }

        var createResponse = await client.PostAsJsonAsync("/api/v1/rest/categories", new { Name = "E2E Category", Description = "E2E category" });
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

        var relistResponse = await client.GetAsync("/api/v1/rest/categories");
        relistResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var relistBody = await relistResponse.Content.ReadAsStringAsync();
        using var relistDoc = JsonDocument.Parse(relistBody);
        var id2 = TryFindFirstCategoryId(relistDoc.RootElement);
        id2.Should().NotBeNullOrEmpty("the created category must be listed");
        return id2!;
    }

    private static string? TryFindFirstCategoryId(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                return ExtractId(item);
            }
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        var id = ExtractId(item);
                        if (!string.IsNullOrEmpty(id))
                        {
                            return id;
                        }
                    }
                }
            }
        }

        return null;
    }

    private static string ExtractId(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals("id", StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.String)
            {
                return property.Value.GetString()!;
            }
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                var nested = ExtractId(property.Value);
                if (!string.IsNullOrEmpty(nested))
                {
                    return nested;
                }
            }
        }

        return string.Empty;
    }

    private static async Task UploadChunkAsync(HttpClient client, string uploadId, byte[] bytes, int chunkIndex, int totalChunks)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(bytes), "chunk", "chunk.bin");
        form.Add(new StringContent(chunkIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)), "chunkIndex");
        form.Add(new StringContent(totalChunks.ToString(System.Globalization.CultureInfo.InvariantCulture)), "totalChunks");

        var response = await client.PostAsync($"/api/v1/videos/upload/chunk/{uploadId}", form);
        var errorBody = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"chunk {chunkIndex} must be accepted: {errorBody}");
    }

    private static async Task<string> WaitForProcessingAsync(HttpClient client, string uploadId, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync($"/api/v1/videos/upload/{uploadId}/status");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            var status = ReadStatus(body);
            if (status is "Completed" or "Failed")
            {
                return status;
            }

            await Task.Delay(500);
        }

        return "Timeout";
    }

    private static string ReadStatus(JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        foreach (var property in body.EnumerateObject())
        {
            if (!property.Name.Equals("status", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.String)
            {
                return property.Value.GetString() ?? string.Empty;
            }

            if (property.Value.ValueKind == JsonValueKind.Number)
            {
                return property.Value.GetInt32() switch
                {
                    0 => "Pending",
                    1 => "Uploading",
                    2 => "Processing",
                    3 => "Completed",
                    4 => "Failed",
                    _ => string.Empty,
                };
            }
        }

        return string.Empty;
    }
}
