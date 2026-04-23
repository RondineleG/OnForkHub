namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using OnForkHub.Core.Responses;

/// <summary>
/// Implementation of IVideoUploadService using HttpClient.
/// </summary>
public sealed class VideoUploadService : IVideoUploadService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoUploadService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls.</param>
    public VideoUploadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    /// <inheritdoc/>
    public async Task<VideoUploadResponse> InitiateUploadAsync(string fileName, long fileSize, string contentType)
    {
        var request = new
        {
            fileName,
            fileSize,
            contentType,
        };
        var response = await _httpClient.PostAsJsonAsync("/api/v1/videos/upload/initiate", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<VideoUploadResponse>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to initiate upload");
    }

    /// <inheritdoc/>
    public async Task<bool> UploadChunkAsync(Guid uploadId, byte[] chunk, int chunkIndex, int totalChunks)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(chunk);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "chunk", "chunk.bin");

        var response = await _httpClient.PostAsync(
            $"/api/v1/videos/upload/chunk/{uploadId}?chunkIndex={chunkIndex}&totalChunks={totalChunks}",
            content
        );

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<string> GetUploadStatusAsync(Guid uploadId)
    {
        var response = await _httpClient.GetAsync($"/api/v1/videos/upload/{uploadId}/status");
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("status").GetString() ?? "Unknown";
    }

    /// <inheritdoc/>
    public async Task<List<VideoUploadResponse>> GetMyUploadsAsync(int page = 1, int pageSize = 20)
    {
        var response = await _httpClient.GetAsync($"/api/v1/videos/upload/my-uploads?page={page}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<VideoUploadResponse>>(_jsonOptions);
        return result ?? [];
    }
}
