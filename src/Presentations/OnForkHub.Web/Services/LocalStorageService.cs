namespace OnForkHub.Web.Services;

using System.Threading.Tasks;
using Microsoft.JSInterop;

/// <summary>
/// Implementation of ILocalStorageService using JavaScript interop.
/// </summary>
public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalStorageService"/> class.
    /// </summary>
    /// <param name="jsRuntime">JavaScript runtime for browser interop.</param>
    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc/>
    public async Task<T?> GetItemAsync<T>(string key)
    {
        return await _jsRuntime.InvokeAsync<T?>("localStorage.getItem", key);
    }

    /// <inheritdoc/>
    public async Task SetItemAsync<T>(string key, T value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value?.ToString());
    }

    /// <inheritdoc/>
    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
