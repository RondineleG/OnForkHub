namespace OnForkHub.Core.Interfaces.Services;

using OnForkHub.Core.Enums;

/// <summary>
/// Service contract for video sharing.
/// </summary>
public interface IShareService
{
    /// <summary>
    /// Generates a sharable link for a video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <returns>A string containing the share link.</returns>
    string GenerateShareLink(Guid videoId);

    /// <summary>
    /// Logic for sharing to a social platform (analytics/tracking).
    /// </summary>
    /// <param name="platform">The target platform.</param>
    /// <param name="url">The URL to share.</param>
    /// <param name="message">Optional message.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ShareToSocialAsync(SocialPlatform platform, string url, string message);
}
