namespace OnForkHub.Application.Services;

using Microsoft.Extensions.Logging;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service for generating sharing links and social media integration.
/// </summary>
public sealed partial class ShareService(ILogger<ShareService> logger) : IShareService
{
    private readonly ILogger<ShareService> _logger = logger;

    /// <inheritdoc/>
    public string GenerateShareLink(Guid videoId)
    {
        return $"/video/{videoId}";
    }

    /// <inheritdoc/>
    public async Task ShareToSocialAsync(SocialPlatform platform, string url, string message)
    {
        LogVideoSharing(_logger, platform, url);
        await Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Sharing video to {Platform}: {Url}")]
    private static partial void LogVideoSharing(ILogger logger, SocialPlatform platform, string url);
}
