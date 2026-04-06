namespace OnForkHub.Web.Services.Api;

using OnForkHub.Web.Models;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Implementation of IVideoService using HttpClient.
/// </summary>
public sealed class VideoService : IVideoService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls.</param>
    public VideoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<VideoDto>> GetVideosAsync(
        int page = 1,
        int pageSize = 12,
        string? search = null,
        long? categoryId = null,
        string? sort = null)
    {
        var url = $"/api/v1/videos?page={page}&size={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        if (categoryId.HasValue)
        {
            url += $"&category={categoryId.Value}";
        }

        if (!string.IsNullOrWhiteSpace(sort))
        {
            url += $"&sort={Uri.EscapeDataString(sort)}";
        }

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<VideoDto>>(_jsonOptions);
        return result ?? new PagedResult<VideoDto>();
    }

    /// <inheritdoc/>
    public async Task<VideoDto?> GetVideoByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/videos/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<VideoDto>(_jsonOptions);
        return result;
    }

    /// <inheritdoc/>
    public async Task<List<VideoDto>> GetRelatedVideosAsync(string videoId, int count = 6)
    {
        var response = await _httpClient.GetAsync($"/api/v1/videos?related={videoId}&size={count}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new List<VideoDto>();
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<VideoDto>>(_jsonOptions);
        return result?.Items ?? new List<VideoDto>();
    }

    /// <inheritdoc/>
    public async Task<VideoDto> UploadVideoAsync(
        Stream fileStream,
        string fileName,
        string title,
        string description,
        long categoryId,
        string tags,
        IProgress<double>? progress = null)
    {
        using var content = new MultipartFormDataContent();

        // Read file stream into byte array for upload
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        content.Add(fileContent, "videoFile", fileName);

        content.Add(new StringContent(title), "title");
        content.Add(new StringContent(description), "description");
        content.Add(new StringContent(categoryId.ToString(System.Globalization.CultureInfo.InvariantCulture)), "categoryId");
        content.Add(new StringContent(tags), "tags");

        var response = await _httpClient.PostAsync("/api/v1/videos/upload", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<VideoDto>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to parse upload response");
    }

    /// <inheritdoc/>
    public async Task DeleteVideoAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/api/v1/videos/{id}");
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public async Task LikeVideoAsync(string id)
    {
        var response = await _httpClient.PostAsync($"/api/v1/videos/{id}/like", content: null);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc/>
    public async Task UnlikeVideoAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/api/v1/videos/{id}/like");
        response.EnsureSuccessStatusCode();
    }
}
