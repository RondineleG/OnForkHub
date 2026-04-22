namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;

using OnForkHub.Core.Responses;

/// <summary>
/// Implementation of IRecommendationService using HttpClient.
/// </summary>
public sealed class RecommendationService(HttpClient httpClient) : IRecommendationService
{
    private readonly HttpClient _httpClient = httpClient;

    /// <inheritdoc/>
    public async Task<List<VideoResponse>> GetRecommendationsAsync(int count = 10)
    {
        var response = await _httpClient.GetAsync($"/api/v1/videos/recommendations?count={count}");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<VideoResponse>>();
        return result ?? [];
    }
}
