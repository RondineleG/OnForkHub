namespace OnForkHub.Application.Dtos.User.Request;

/// <summary>
/// Request DTO for token refresh.
/// </summary>
public sealed class RefreshTokenRequestDto
{
    /// <summary>
    /// Gets or sets the expired access token.
    /// </summary>
    [Required(ErrorMessage = "Access token is required")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}
