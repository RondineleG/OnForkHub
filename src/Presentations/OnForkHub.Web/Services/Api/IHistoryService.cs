namespace OnForkHub.Web.Services.Api;

using OnForkHub.Web.Models;

/// <summary>
/// Service contract for user viewing history operations.
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// Gets the user's viewing history.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Video>> GetHistoryAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Adds a video to the user's viewing history.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> AddToHistoryAsync(Guid videoId);
}
