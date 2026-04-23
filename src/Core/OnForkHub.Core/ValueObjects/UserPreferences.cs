namespace OnForkHub.Core.ValueObjects;

/// <summary>
/// Represents user specific preferences and settings.
/// </summary>
public sealed class UserPreferences : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserPreferences"/> class.
    /// </summary>
    public UserPreferences()
    {
        AutoPlayNextVideo = true;
        DefaultQuality = "Auto";
        EnableP2P = true;
        DownloadLimitMb = 0; // 0 = Unlimited
        UploadLimitMb = 0; // 0 = Unlimited
        DarkMode = false;
        Language = "pt-BR";
    }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically play the next video.
    /// </summary>
    public bool AutoPlayNextVideo { get; set; }

    /// <summary>
    /// Gets or sets the default video quality preference.
    /// </summary>
    public string DefaultQuality { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether P2P (WebTorrent) is enabled.
    /// </summary>
    public bool EnableP2P { get; set; }

    /// <summary>
    /// Gets or sets the download limit in MB/s for P2P.
    /// </summary>
    public double DownloadLimitMb { get; set; }

    /// <summary>
    /// Gets or sets the upload limit in MB/s for P2P.
    /// </summary>
    public double UploadLimitMb { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether dark mode is enabled in the UI.
    /// </summary>
    public bool DarkMode { get; set; }

    /// <summary>
    /// Gets or sets the preferred UI language.
    /// </summary>
    public string Language { get; set; }

    /// <inheritdoc/>
    public override ValidationResult Validate() => ValidationResult.Success();

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AutoPlayNextVideo;
        yield return DefaultQuality;
        yield return EnableP2P;
        yield return DownloadLimitMb;
        yield return UploadLimitMb;
        yield return DarkMode;
        yield return Language;
    }
}
