namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;

using OnForkHub.Core.Responses;
using OnForkHub.Web.Models;

/// <summary>
/// Implementation of IVideoService using HttpClient for REST API communication.
/// </summary>
public sealed class VideoService(HttpClient httpClient) : IVideoService
{
    private readonly HttpClient _httpClient = httpClient;

    /// <inheritdoc/>
    public async Task<List<Video>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null, long? categoryId = null, string? sort = null)
    {
        var url = $"/api/v1/rest/videos?page={page}&itemsPerPage={pageSize}";
        if (!string.IsNullOrEmpty(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        if (categoryId.HasValue)
        {
            url += $"&categoryId={categoryId}";
        }

        if (!string.IsNullOrEmpty(sort))
        {
            url += $"&sort={Uri.EscapeDataString(sort)}";
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url);
        }
        catch
        {
            return [];
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            // Tenta fallback para rota sem "rest/" se necessário
            var fallbackUrl = $"/api/v1/videos?page={page}&itemsPerPage={pageSize}";
            try
            {
                response = await _httpClient.GetAsync(fallbackUrl);
                if (!response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return [];
                }
            }
            catch
            {
                return [];
            }
        }

        // 1. Tenta deserializar como envelope ApiEnvelope<List<ApiVideoItem>>
        try
        {
            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<List<ApiVideoItem>>>();
            if (envelope?.Data is { Count: > 0 })
            {
                return envelope.Data.Select(x => x.ToVideo()).ToList();
            }
        }
        catch
        {
            // Silenciosamente tenta fallback direto
        }

        // 2. Tenta deserializar como lista direta de ApiVideoItem
        try
        {
            var directList = await response.Content.ReadFromJsonAsync<List<ApiVideoItem>>();
            if (directList is { Count: > 0 })
            {
                return directList.Select(x => x.ToVideo()).ToList();
            }
        }
        catch
        {
        }

        // 3. Tenta deserializar como lista de VideoResponse legado
        try
        {
            var legacyList = await response.Content.ReadFromJsonAsync<List<VideoResponse>>();
            if (legacyList is { Count: > 0 })
            {
                return legacyList.Select(MapToWebModel).ToList();
            }
        }
        catch
        {
        }

        return [];
    }

    /// <inheritdoc/>
    public async Task<Video?> GetByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/rest/videos/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Fallback para rota sem rest/
            response = await _httpClient.GetAsync($"/api/v1/videos/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        // 1. Tenta envelope
        try
        {
            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<ApiVideoItem>>();
            if (envelope?.Data is not null)
            {
                return envelope.Data.ToVideo();
            }
        }
        catch
        {
        }

        // 2. Tenta direto
        try
        {
            var direct = await response.Content.ReadFromJsonAsync<ApiVideoItem>();
            if (direct is not null)
            {
                return direct.ToVideo();
            }
        }
        catch
        {
        }

        // 3. Fallback VideoResponse
        try
        {
            var legacy = await response.Content.ReadFromJsonAsync<VideoResponse>();
            return legacy != null ? MapToWebModel(legacy) : null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse<Video>> SearchAsync(string? q, long? categoryId = null, int page = 1, int pageSize = 20)
    {
        var url = $"/api/v1/videos/search?q={Uri.EscapeDataString(q ?? string.Empty)}&page={page}&pageSize={pageSize}";
        if (categoryId.HasValue)
        {
            url += $"&categoryId={categoryId}";
        }

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return new SearchResponse<Video>();
        }

        var result = await response.Content.ReadFromJsonAsync<SearchResponse<VideoResponse>>();
        return new SearchResponse<Video>
        {
            Items = result?.Items?.Select(MapToWebModel).ToList() ?? [],
            TotalCount = result?.TotalCount ?? 0,
            Page = result?.Page ?? 1,
            PageSize = result?.PageSize ?? 20,
        };
    }

    /// <inheritdoc/>
    public async Task IncrementViewCountAsync(Guid videoId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/v1/videos/{videoId}/views", null);
            if (!response.IsSuccessStatusCode)
            {
                await _httpClient.PostAsync($"/api/v1/rest/videos/{videoId}/views", null);
            }
        }
        catch
        {
            // Views increment fire-and-forget
        }
    }

    // Compatibility implementations
    public async Task<List<Video>> GetVideosAsync(int page = 1, int pageSize = 10) => await GetAllAsync(page, pageSize);

    public async Task<Video?> GetVideoByIdAsync(string id) => await GetByIdAsync(id);

    public async Task<List<Video>> GetRelatedVideosAsync(string videoId, int count = 5) => await GetAllAsync(1, count);

    public async Task<bool> LikeVideoAsync(string id) => await Task.FromResult(true);

    public async Task<bool> UnlikeVideoAsync(string id) => await Task.FromResult(true);

    public async Task<bool> DeleteVideoAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/api/v1/rest/videos/{id}");
        if (!response.IsSuccessStatusCode)
        {
            response = await _httpClient.DeleteAsync($"/api/v1/videos/{id}");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UploadVideoAsync(Stream fileStream, string fileName, string title, string description, string categoryId)
    {
        // This legacy entry point is not wired to the chunked upload and catalog creation flow.
        // Never report success without persisting the video.
        return await Task.FromResult(false);
    }

    private static Video MapToWebModel(VideoResponse response)
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
            MagnetUri = response.MagnetUri,
            ViewCount = response.ViewCount,
        };
    }
}
