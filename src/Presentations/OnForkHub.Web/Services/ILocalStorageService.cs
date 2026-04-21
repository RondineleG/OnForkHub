namespace OnForkHub.Web.Services;

using System.Threading.Tasks;

/// <summary>
/// Service for interacting with browser local storage.
/// </summary>
public interface ILocalStorageService
{
    /// <summary>
    /// Gets an item from local storage.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="key">The key of the item.</param>
    /// <returns>The item value, or default if not found.</returns>
    Task<T?> GetItemAsync<T>(string key);

    /// <summary>
    /// Sets an item in local storage.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="key">The key of the item.</param>
    /// <param name="value">The value to store.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task SetItemAsync<T>(string key, T value);

    /// <summary>
    /// Removes an item from local storage.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task RemoveItemAsync(string key);
}
