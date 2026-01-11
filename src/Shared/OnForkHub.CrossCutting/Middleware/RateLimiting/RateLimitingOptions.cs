namespace OnForkHub.CrossCutting.Middleware.RateLimiting;

/// <summary>
/// Configuration options for rate limiting.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of requests per window for the default policy.
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time window in seconds.
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the queue limit for pending requests.
    /// </summary>
    public int QueueLimit { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of requests for authenticated users.
    /// </summary>
    public int AuthenticatedPermitLimit { get; set; } = 500;

    /// <summary>
    /// Gets or sets the maximum number of requests for anonymous users.
    /// </summary>
    public int AnonymousPermitLimit { get; set; } = 50;
}
