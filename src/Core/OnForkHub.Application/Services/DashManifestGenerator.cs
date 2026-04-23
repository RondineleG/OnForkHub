namespace OnForkHub.Application.Services;

using System.Globalization;
using System.Text;

/// <summary>
/// Generator for MPEG-DASH manifests.
/// </summary>
public sealed class DashManifestGenerator
{
    /// <summary>
    /// Generates a simple MPEG-DASH manifest for a video.
    /// </summary>
    /// <param name="baseUrl">Base URL for video segments.</param>
    /// <param name="videoId">Video identifier.</param>
    /// <returns>XML manifest content.</returns>
    public static string GenerateManifest(string baseUrl, Guid videoId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<MPD xmlns=\"urn:mpeg:dash:schema:mpd:2011\" type=\"static\" mediaPresentationDuration=\"PT2M\">");
        sb.AppendLine("    <Period>");
        sb.AppendLine("        <AdaptationSet mimeType=\"video/mp4\" codecs=\"avc1.42E01E\">");

        // 1080p Representation
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"            <Representation id=\"1080p\" bandwidth=\"5000000\" width=\"1920\" height=\"1080\">"
        );
        sb.AppendLine(CultureInfo.InvariantCulture, $"                <BaseURL>{baseUrl}/api/v1/videos/{videoId}/stream?quality=1080p</BaseURL>");
        sb.AppendLine("                <SegmentTemplate timescale=\"1000\" duration=\"4000\" />");
        sb.AppendLine("            </Representation>");

        // 720p Representation
        sb.AppendLine(CultureInfo.InvariantCulture, $"            <Representation id=\"720p\" bandwidth=\"2500000\" width=\"1280\" height=\"720\">");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                <BaseURL>{baseUrl}/api/v1/videos/{videoId}/stream?quality=720p</BaseURL>");
        sb.AppendLine("                <SegmentTemplate timescale=\"1000\" duration=\"4000\" />");
        sb.AppendLine("            </Representation>");

        // 480p Representation
        sb.AppendLine(CultureInfo.InvariantCulture, $"            <Representation id=\"480p\" bandwidth=\"1000000\" width=\"854\" height=\"480\">");
        sb.AppendLine(CultureInfo.InvariantCulture, $"                <BaseURL>{baseUrl}/api/v1/videos/{videoId}/stream?quality=480p</BaseURL>");
        sb.AppendLine("                <SegmentTemplate timescale=\"1000\" duration=\"4000\" />");
        sb.AppendLine("            </Representation>");

        sb.AppendLine("        </AdaptationSet>");
        sb.AppendLine("    </Period>");
        sb.AppendLine("</MPD>");

        return sb.ToString();
    }
}
