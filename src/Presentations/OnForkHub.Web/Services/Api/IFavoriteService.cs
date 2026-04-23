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
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A list of favorite videos.</returns>
    Task<List<Video>> GetFavoritesAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Adds a video to the user's favorites.
    /// </summary>
    /// <param name="videoId">The ID of the video to add.</param>
    /// <returns>True if the operation was successful, otherwise false.</returns>
    Task<bool> AddToFavoritesAsync(Guid videoId);

    /// <summary>
    /// Removes a video from the user's favorites.
    /// </summary>
    /// <param name="videoId">The ID of the video to remove.</param>
    /// <returns>True if the operation was successful, otherwise false.</returns>
    Task<bool> RemoveFromFavoritesAsync(Guid videoId);

    /// <summary>
    /// Checks if a video is in the user's favorites.
    /// </summary>
    /// <param name="videoId">The ID of the video to check.</param>
    /// <returns>True if the video is favorited, otherwise false.</returns>
    Task<bool> IsFavoriteAsync(Guid videoId);
}
