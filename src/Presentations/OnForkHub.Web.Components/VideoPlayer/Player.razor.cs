// The .NET Foundation licenses this file to you under the MIT license.

using OnForkHub.Web.Components.Models;

namespace OnForkHub.Web.Components.VideoPlayer;

public partial class Player
{
    private DotNetObjectReference<Player> objectRef = null!;

    [Parameter]
    public bool AirplayControl { get; set; } = false;

    [Parameter]
    public bool Captions { get; set; } = false;

    [Parameter]
    public bool CaptionsControl { get; set; } = false;

    [Parameter]
    public bool CurrentTimeControl { get; set; } = false;

    [Parameter]
    public bool DownloadControl { get; set; } = false;

    [Parameter]
    public bool DurationControl { get; set; } = false;

    [Parameter]
    public bool FastForwardControl { get; set; } = false;

    [Parameter]
    public bool FullscreenControl { get; set; } = false;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> InputAttributes { get; set; } = [];

    [Parameter]
    public bool Loop { get; set; } = false;

    [Parameter]
    public string MagnetUri { get; set; } = string.Empty;

    [Parameter]
    public bool MuteControl { get; set; } = false;

    [Parameter]
    public EventCallback OnEndedVideo { get; set; }

    [Parameter]
    public EventCallback OnPlayVideo { get; set; }

    [Parameter]
    public EventCallback<(float currentTime, float duration)> OnVideoTimeUpdate { get; set; }

    [Parameter]
    public bool PIPControl { get; set; } = false;

    [Parameter]
    public bool PlayControl { get; set; } = false;

    [Parameter]
    public bool PlayLargeControl { get; set; } = false;

    [Parameter]
    public string Poster { get; set; } = string.Empty;

    [Parameter]
    public bool ProgressControl { get; set; } = false;

    [Parameter]
    public bool Quality { get; set; } = false;

    [Parameter]
    public bool RestartControl { get; set; } = false;

    [Parameter]
    public bool RewindControl { get; set; } = false;

    [Parameter]
    public bool SettingsControl { get; set; } = false;

    [Parameter]
    public List<Source> Sources { get; set; } = [];

    [Parameter]
    public bool Speed { get; set; } = false;

    [Parameter]
    public List<Track> Tracks { get; set; } = [];

    [Parameter]
    public bool VolumeControl { get; set; } = false;

    [JSInvokable]
    public async Task OnEnded()
    {
        await OnEndedVideo.InvokeAsync();
    }

    [JSInvokable]
    public async Task OnPlay()
    {
        await OnPlayVideo.InvokeAsync();
    }

    [JSInvokable]
    public async Task OnTimeUpdate(float currentTime, float duration)
    {
        await OnVideoTimeUpdate.InvokeAsync((currentTime, duration));
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        objectRef = DotNetObjectReference.Create(this);

        if (
            InputAttributes == null
            || !InputAttributes.Any(p => p.Key == "id")
            || InputAttributes.FirstOrDefault(p => p.Key == "id").Value == null
            || string.IsNullOrWhiteSpace(InputAttributes.FirstOrDefault(p => p.Key == "id").Value.ToString())
        )
        {
            throw new ArgumentException("id (HTML) can not be null or empty");
        }

        var elementId = InputAttributes.FirstOrDefault(p => p.Key == "id").Value.ToString();

        await VideoPlayerService.Initialize(
            elementId!,
            objectRef,
            MagnetUri,
            Captions,
            Quality,
            Speed,
            Loop,
            PlayLargeControl,
            RestartControl,
            RewindControl,
            PlayControl,
            FastForwardControl,
            ProgressControl,
            CurrentTimeControl,
            DurationControl,
            MuteControl,
            VolumeControl,
            CaptionsControl,
            SettingsControl,
            PIPControl,
            AirplayControl,
            DownloadControl,
            FullscreenControl
        );
    }
}
