namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;

using OnForkHub.Core.Responses;
using OnForkHub.Web.Models;

/// <summary>
/// Implementation of IVideoService using HttpClient.
/// </summary>
public sealed class VideoService(HttpClient httpClient) : IVideoService
{
    private readonly HttpClient _httpClient = httpClient;

    /// <inheritdoc/>
    public async Task<List<Video>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null, long? categoryId = null, string? sort = null)
    {
        var url = $"/api/v1/videos?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search))
            url += $"&search={search}";
        if (categoryId.HasValue)
            url += $"&categoryId={categoryId}";
        if (!string.IsNullOrEmpty(sort))
            url += $"&sort={sort}";

        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<VideoResponse>>();
        return result?.Select(MapToWebModel).ToList() ?? [];
    }

    /// <inheritdoc/>
    public async Task<Video?> GetByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/videos/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<VideoResponse>();
        return result != null ? MapToWebModel(result) : null;
    }

    /// <inheritdoc/>
    public async Task<SearchResponse<Video>> SearchAsync(string? q, long? categoryId = null, int page = 1, int pageSize = 20)
    {
        var url = $"/api/v1/videos/search?q={q}&page={page}&pageSize={pageSize}";
        if (categoryId.HasValue)
            url += $"&categoryId={categoryId}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SearchResponse<VideoResponse>>();
        return new SearchResponse<Video>
        {
            Items = result?.Items?.Select(MapToWebModel).ToList() ?? [],
            TotalCount = result?.TotalCount ?? 0,
            Page = result?.Page ?? 1,
            PageSize = result?.PageSize ?? 20,
        };
    }

    // Compatibility implementations
    public async Task<List<Video>> GetVideosAsync(int page = 1, int pageSize = 10) => await GetAllAsync(page, pageSize);

    public async Task<Video?> GetVideoByIdAsync(string id) => await GetByIdAsync(id);

    public async Task<List<Video>> GetRelatedVideosAsync(string videoId, int count = 5) => await GetAllAsync(1, count); // Simple mock

    public async Task<bool> LikeVideoAsync(string id) => true; // Use RatingService in real scenario

    public async Task<bool> UnlikeVideoAsync(string id) => true;

    public async Task<bool> DeleteVideoAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/api/v1/videos/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UploadVideoAsync(Stream fileStream, string fileName, string title, string description, string categoryId)
    {
        // Simple mock for existing component
        return true;
    }

    private Video MapToWebModel(VideoResponse response)
    {
        return new Video
        {
            Id = Guid.TryParse(response.Id, out var guid) ? guid : Guid.Empty,
            Title = response.Title,
            Name = response.Title,
            Description = response.Description,
            Url = response.Url,
            Thumbnail = response.ThumbnailUrl,
            Duration = response.Duration,
            CreatedAt = response.CreatedAt,
            IsTorrent = response.IsTorrentEnabled,
        };
    }
}
