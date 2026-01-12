namespace OnForkHub.CrossCutting.Authentication;

/// <summary>
/// Represents the result of a token generation operation.
/// </summary>
public sealed class TokenResult
{
    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token expiration time.
    /// </summary>
    public DateTime AccessTokenExpiration { get; set; }

    /// <summary>
    /// Gets or sets the refresh token expiration time.
    /// </summary>
    public DateTime RefreshTokenExpiration { get; set; }

    /// <summary>
    /// Gets or sets the token type.
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}
