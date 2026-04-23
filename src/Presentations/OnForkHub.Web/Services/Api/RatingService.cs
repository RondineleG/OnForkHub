namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Requests.Videos;

/// <summary>
/// Implementation of IRatingService using HttpClient.
/// </summary>
public sealed class RatingService(HttpClient httpClient) : IRatingService
{
    private readonly HttpClient _httpClient = httpClient;

    /// <inheritdoc/>
    public async Task<bool> SetRatingAsync(Guid videoId, ERatingType type)
    {
        var request = new SetRatingRequest(type);
        var response = await _httpClient.PostAsJsonAsync($"/api/v1/videos/{videoId}/rating", request);
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveRatingAsync(Guid videoId)
    {
        var response = await _httpClient.DeleteAsync($"/api/v1/videos/{videoId}/rating");
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<VideoRatingStats> GetStatsAsync(Guid videoId)
    {
        var response = await _httpClient.GetAsync($"/api/v1/videos/{videoId}/rating");
        if (!response.IsSuccessStatusCode)
        {
            return new VideoRatingStats();
        }

        return await response.Content.ReadFromJsonAsync<VideoRatingStats>() ?? new VideoRatingStats();
    }
}
