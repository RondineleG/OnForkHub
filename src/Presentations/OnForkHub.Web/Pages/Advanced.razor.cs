using OnForkHub.Web.Components.VideoPlayer;

namespace OnForkHub.Web.Pages;

public partial class Advanced
{
    private readonly List<Source> sources = [new() { Src = "/sample.mp4", Type = "video/mp4" }];

    private static void OnEndedVideo()
    {
        Console.WriteLine("End of play");
    }

    private static void OnPlayVideo()
    {
        Console.WriteLine("Start playing");
    }

    private static void OnVideoTimeUpdate((float currentTime, float duration) timeInfo)
    {
        Console.WriteLine($"Current time: {timeInfo.currentTime}, Duration: {timeInfo.duration}");
    }
}
