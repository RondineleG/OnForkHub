namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;

using OnForkHub.Core.Responses;
using OnForkHub.Web.Models;

/// <summary>
/// Implementation of IFavoriteService using HttpClient.
/// </summary>
public sealed class FavoriteService(HttpClient httpClient) : IFavoriteService
{
    private readonly HttpClient _httpClient = httpClient;

    /// <inheritdoc/>
    public async Task<List<Video>> GetFavoritesAsync(int page = 1, int pageSize = 20)
    {
        var response = await _httpClient.GetAsync($"/api/v1/users/favorites?page={page}&pageSize={pageSize}");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<VideoResponse>>();
        return result?.Select(MapToWebModel).ToList() ?? [];
    }

    /// <inheritdoc/>
    public async Task<bool> AddToFavoritesAsync(Guid videoId)
    {
        var response = await _httpClient.PostAsync($"/api/v1/users/favorites/{videoId}", null);
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveFromFavoritesAsync(Guid videoId)
    {
        var response = await _httpClient.DeleteAsync($"/api/v1/users/favorites/{videoId}");
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> IsFavoriteAsync(Guid videoId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/users/favorites/{videoId}/check");
            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }
        catch
        {
            return false;
        }
    }

    private Video MapToWebModel(VideoResponse response)
    {
        return new Video
        {
            Id = Guid.TryParse(response.Id, out var guid) ? guid : Guid.Empty,
            Title = response.Title,
            Description = response.Description,
            Url = response.Url,
            Thumbnail = response.ThumbnailUrl,
            Duration = response.Duration,
            CreatedAt = response.CreatedAt,
            IsTorrent = response.IsTorrentEnabled,
            ViewCount = response.ViewCount,
        };
    }
}
