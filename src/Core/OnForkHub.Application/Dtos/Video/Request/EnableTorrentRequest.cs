namespace OnForkHub.Application.Dtos.Video.Request;

/// <summary>
/// Request to enable torrent for a video.
/// </summary>
/// <param name="MagnetUri">The magnet URI.</param>
public record EnableTorrentRequest(string MagnetUri);
