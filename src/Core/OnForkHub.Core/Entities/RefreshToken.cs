namespace OnForkHub.Core.Entities;

/// <summary>
/// Entity representing a JWT refresh token stored in the database.
/// </summary>
public class RefreshToken : BaseEntity
{
    private const int MaxTokenLength = 500;
    private const int MaxUserAgentLength = 500;
    private const int MaxIpAddressLength = 45;

    /// <summary>
    /// Gets the raw token value used to identify this refresh token.
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user ID this token belongs to.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date and time when this token expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the date and time when this token was revoked (null if still active).
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Gets the IP address that created this token.
    /// </summary>
    public string CreatedByIp { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user agent string from the request that created this token.
    /// </summary>
    public string UserAgent { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue || ExpiresAt <= DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class.
    /// Private constructor for EF Core.
    /// </summary>
    private RefreshToken() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The value object identifier.</param>
    /// <param name="token">The raw token value.</param>
    /// <param name="userId">The user ID this token belongs to.</param>
    /// <param name="expiresAt">The expiration date and time.</param>
    /// <param name="createdByIp">The IP address that created this token.</param>
    /// <param name="userAgent">The user agent string.</param>
    private RefreshToken(Id id, string token, string userId, DateTime expiresAt, string createdByIp, string userAgent)
        : base(id, DateTime.UtcNow)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        UserAgent = userAgent;
    }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    /// <param name="token">The raw token value.</param>
    /// <param name="userId">The user ID this token belongs to.</param>
    /// <param name="expiresAt">The expiration date and time.</param>
    /// <param name="createdByIp">The IP address that created this token.</param>
    /// <param name="userAgent">The user agent string.</param>
    /// <returns>A request result containing the created refresh token.</returns>
    public static RequestResult<RefreshToken> Create(string token, string userId, DateTime expiresAt, string createdByIp, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return RequestResult<RefreshToken>.WithError("Token is required");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return RequestResult<RefreshToken>.WithError("User ID is required");
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            return RequestResult<RefreshToken>.WithError("Expiration date must be in the future");
        }

        var id = ValueObjects.Id.Create();
        var refreshToken = new RefreshToken(id, token, userId, expiresAt, createdByIp, userAgent);
        return RequestResult<RefreshToken>.Success(refreshToken);
    }

    /// <summary>
    /// Revokes this refresh token.
    /// </summary>
    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}
