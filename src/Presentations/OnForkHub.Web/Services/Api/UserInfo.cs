namespace OnForkHub.Web.Services.Api;

using System.Collections.Generic;

/// <summary>
/// User information from authentication.
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
