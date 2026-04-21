namespace OnForkHub.Application.Dtos.User.Response;

/// <summary>
/// Data transfer object for authentication responses.
/// </summary>
public sealed class AuthResponseDto
{
    /// <summary>
    /// Gets or sets the user information.
    /// </summary>
    public UserResponseDto User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token expiration date.
    /// </summary>
    public DateTime AccessTokenExpiration { get; set; }

    /// <summary>
    /// Gets or sets the refresh token expiration date.
    /// </summary>
    public DateTime RefreshTokenExpiration { get; set; }
}
