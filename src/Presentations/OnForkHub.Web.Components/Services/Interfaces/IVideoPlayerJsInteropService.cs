// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Web.Components.Services.Interfaces;

public interface IVideoPlayerJsInteropService
{
    Task Initialize(
        string id,
        DotNetObjectReference<Player> objectRef,
        string magnetUri,
        bool captions,
        bool quality,
        bool speed,
        bool loop,
        bool playLargeControl,
        bool restartControl,
        bool rewindControl,
        bool playControl,
        bool fastForwardControl,
        bool progressControl,
        bool currentTimeControl,
        bool durationControl,
        bool muteControl,
        bool volumeControl,
        bool captionsControl,
        bool settingsControl,
        bool pIPControl,
        bool airplayControl,
        bool downloadControl,
        bool fullscreenControl
    );
}
