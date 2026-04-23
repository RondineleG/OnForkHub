namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;

using OnForkHub.Core.Requests.Videos;

/// <summary>
/// Implementation of ICommentService using HttpClient.
/// </summary>
public sealed class CommentService(HttpClient httpClient) : ICommentService
{
    private readonly HttpClient _httpClient = httpClient;

    /// <inheritdoc/>
    public async Task<List<CommentDisplayModel>> GetCommentsAsync(Guid videoId, int page = 1, int pageSize = 20)
    {
        var response = await _httpClient.GetAsync($"/api/v1/videos/{videoId}/comments?page={page}&pageSize={pageSize}");
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CommentListResponse>();
        return result?.Comments ?? [];
    }

    /// <inheritdoc/>
    public async Task<bool> CreateCommentAsync(Guid videoId, CreateCommentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/v1/videos/{videoId}/comments", request);
        return response.IsSuccessStatusCode;
    }

    private sealed class CommentListResponse
    {
        public List<CommentDisplayModel> Comments { get; set; } = [];
    }
}
