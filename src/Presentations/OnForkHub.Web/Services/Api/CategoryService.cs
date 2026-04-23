namespace OnForkHub.Web.Services.Api;

using System.Net.Http.Json;
using System.Text.Json;

using OnForkHub.Web.Models;

/// <summary>
/// Implementation of ICategoryService using HttpClient.
/// </summary>
public sealed class CategoryService : ICategoryService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls.</param>
    public CategoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var url = $"/api/v1/categories?page={page}&size={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<CategoryDto>>(_jsonOptions);
        return result ?? new PagedResult<CategoryDto>();
    }

    /// <inheritdoc/>
    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/categories?size=1000");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new List<CategoryDto>();
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<CategoryDto>>(_jsonOptions);
        return result?.Items ?? new List<CategoryDto>();
    }

    /// <inheritdoc/>
    public async Task<CategoryDto?> GetCategoryByIdAsync(long id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/categories/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CategoryDto>(_jsonOptions);
        return result;
    }

    /// <inheritdoc/>
    public async Task<CategoryDto> CreateCategoryAsync(string name, string description)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/categories", new { Name = name, Description = description });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CategoryDto>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to parse create category response");
    }

    /// <inheritdoc/>
    public async Task<CategoryDto> UpdateCategoryAsync(long id, string name, string description)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/v1/categories/{id}", new { Name = name, Description = description });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CategoryDto>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to parse update category response");
    }

    /// <inheritdoc/>
    public async Task DeleteCategoryAsync(long id)
    {
        var response = await _httpClient.DeleteAsync($"/api/v1/categories/{id}");
        response.EnsureSuccessStatusCode();
    }
}
