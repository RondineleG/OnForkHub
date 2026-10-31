namespace OnForkHub.Web.Models;

/// <summary>
/// A video shown in the shorts feed, streamed peer-to-peer from a torrent.
/// </summary>
/// <param name="Title">Display title.</param>
/// <param name="Description">One-line synopsis shown over the video.</param>
/// <param name="Credit">Author and license line.</param>
/// <param name="TorrentUrl">Magnet URI or http(s) URL of a .torrent file.</param>
public sealed record ShortVideo(string Title, string Description, string Credit, string TorrentUrl);
