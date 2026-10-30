using OnForkHub.Web.Components.Models;

namespace OnForkHub.Web.Components.VideoPlayer;

public partial class Player : ComponentBase, IAsyncDisposable
{
    private DotNetObjectReference<Player> _objectRef = null!;

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
    public bool EnableTorrentFileUpload { get; set; } = false;

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
    public EventCallback<string> OnTorrentError { get; set; }

    [Parameter]
    public EventCallback<int> OnTorrentProgress { get; set; }

    [Parameter]
    public EventCallback OnTorrentReady { get; set; }

    [Parameter]
    public EventCallback<(float CurrentTime, float Duration)> OnVideoTimeUpdate { get; set; }

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
    public string TorrentFilePath { get; set; } = string.Empty;

    [Parameter]
    public List<Track> Tracks { get; set; } = [];

    [Parameter]
    public bool VolumeControl { get; set; } = false;

    private string ErrorMessage { get; set; } = string.Empty;

    private double TorrentDownloadSpeed { get; set; }

    private string TorrentFileName { get; set; } = string.Empty;

    private int TorrentPeers { get; set; }

    private int TorrentProgress { get; set; }

    private ETorrentState TorrentState { get; set; } = ETorrentState.Idle;

    public ValueTask DisposeAsync()
    {
        _objectRef?.Dispose();

        try
        {
            // Cleanup JavaScript resources
            _ = Task.Run(async () =>
            {
                try
                {
                    await VideoPlayerService.CleanupTorrent();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao limpar recursos do torrent: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao fazer dispose do VideoPlayerService: {ex.Message}");
        }

        return default;
    }

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

    [JSInvokable]
    public async Task OnTorrentErrorCallback(string error)
    {
        ErrorMessage = error;
        TorrentState = ETorrentState.Error;
        await InvokeAsync(StateHasChanged);
        await OnTorrentError.InvokeAsync(error);
    }

    [JSInvokable]
    public async Task OnTorrentReadyCallback(string videoName)
    {
        ErrorMessage = string.Empty;
        TorrentFileName = videoName;
        if (TorrentState != ETorrentState.Completed)
        {
            TorrentState = ETorrentState.Playing;
        }

        await InvokeAsync(StateHasChanged);
        await OnTorrentReady.InvokeAsync();
    }

    [JSInvokable]
    public async Task OnTorrentStatsUpdate(int progress, int peers, double downloadSpeed)
    {
        TorrentProgress = progress;
        TorrentPeers = peers;
        TorrentDownloadSpeed = downloadSpeed;
        if (progress >= 100)
        {
            TorrentState = ETorrentState.Completed;
        }
        else if (TorrentState == ETorrentState.Idle)
        {
            TorrentState = ETorrentState.Connecting;
        }

        await InvokeAsync(StateHasChanged);
        await OnTorrentProgress.InvokeAsync(progress);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _objectRef = DotNetObjectReference.Create(this);

        if (
            InputAttributes == null
            || !InputAttributes.TryGetValue("id", out var elementValue)
            || elementValue == null
            || string.IsNullOrWhiteSpace(elementValue.ToString())
        )
        {
            throw new ArgumentException("O atributo 'id' é obrigatório e não pode ser nulo ou vazio");
        }

        var elementIdentifier = elementValue.ToString()!;

        try
        {
            await VideoPlayerService.Initialize(
                elementIdentifier,
                _objectRef,
                MagnetUri,
                TorrentFilePath,
                EnableTorrentFileUpload,
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
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao inicializar player: {ex.Message}";
            await InvokeAsync(StateHasChanged);
        }
    }

    private static string FormatSpeed(double bytesPerSecond) =>
        bytesPerSecond >= 1024 * 1024 ? $"{bytesPerSecond / (1024 * 1024):0.0} MB/s" : $"{bytesPerSecond / 1024:0} KB/s";

    // Starts as soon as a file is picked: selecting the .torrent is the only action the user has to take.
    private async Task HandleTorrentFile(ChangeEventArgs e)
    {
        var selectedPath = e.Value?.ToString();
        if (string.IsNullOrEmpty(selectedPath))
        {
            return;
        }

        ErrorMessage = string.Empty;
        TorrentFileName = Path.GetFileName(selectedPath.Replace('\\', '/'));
        TorrentProgress = 0;
        TorrentPeers = 0;
        TorrentDownloadSpeed = 0;
        TorrentState = ETorrentState.Connecting;

        try
        {
            if (InputAttributes != null && InputAttributes.TryGetValue("id", out var elementValue))
            {
                var elementIdentifier = elementValue.ToString()!;
                await VideoPlayerService.StartTorrentFromFile(elementIdentifier, _objectRef);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar arquivo torrent: {ex.Message}";
            TorrentState = ETorrentState.Error;
            await InvokeAsync(StateHasChanged);
        }
    }
}
