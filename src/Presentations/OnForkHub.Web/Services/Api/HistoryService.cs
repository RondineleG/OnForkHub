namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;

using OnForkHub.Core.Responses;
using OnForkHub.Web.Models;

/// <summary>
/// Implementation of IHistoryService using HttpClient.
/// </summary>
public sealed class HistoryService(HttpClient httpClient) : IHistoryService
{
    private readonly HttpClient _httpClient = httpClient;

    /// <inheritdoc/>
    public async Task<List<Video>> GetHistoryAsync(int page = 1, int pageSize = 20)
    {
        var response = await _httpClient.GetAsync($"/api/v1/users/history?page={page}&pageSize={pageSize}");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<VideoResponse>>();
        return result?.Select(MapToWebModel).ToList() ?? [];
    }

    /// <inheritdoc/>
    public async Task<bool> AddToHistoryAsync(Guid videoId)
    {
        var response = await _httpClient.PostAsync($"/api/v1/users/history/{videoId}", null);
        return response.IsSuccessStatusCode;
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
