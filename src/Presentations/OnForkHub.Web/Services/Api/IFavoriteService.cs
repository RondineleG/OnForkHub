namespace OnForkHub.Web.Services.Api;

using OnForkHub.Web.Models;

/// <summary>
/// Service contract for user favorite video operations.
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// Gets the list of favorite videos for the current user.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Video>> GetFavoritesAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Adds a video to the user's favorites.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> AddToFavoritesAsync(Guid videoId);

    /// <summary>
    /// Removes a video from the user's favorites.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> RemoveFromFavoritesAsync(Guid videoId);
}
