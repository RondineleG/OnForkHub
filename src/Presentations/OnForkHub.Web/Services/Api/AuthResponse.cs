namespace OnForkHub.Web.Services.Api;

using System;
using System.Collections.Generic;

/// <summary>
/// Response from authentication operations.
/// </summary>
public class AuthResponse
{
    public UserInfo User { get; set; } = new();
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}
