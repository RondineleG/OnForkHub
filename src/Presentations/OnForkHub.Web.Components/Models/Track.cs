// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Web.Components.Models;

public class Track
{
    public bool IsDefault { get; set; }

    public string Kind { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Src { get; set; } = string.Empty;

    public string SrcLang { get; set; } = string.Empty;
}
