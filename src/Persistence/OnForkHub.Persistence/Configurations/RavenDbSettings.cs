// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Configurations;

public class RavenDbSettings
{
    public string Database { get; set; } = string.Empty;

    public string[] Urls { get; set; } = [];
}
