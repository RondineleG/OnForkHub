namespace OnForkHub.Web.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Modelo intermediário flexível para desserialização de vídeos retornados pela API REST.
/// Suporta propriedades primitivas (string, guid) ou Value Objects com propriedade 'value'.
/// </summary>
public sealed class ApiVideoItem
{
    [JsonPropertyName("id")]
    public JsonElement IdElement { get; set; }

    [JsonPropertyName("title")]
    public JsonElement TitleElement { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public JsonElement UrlElement { get; set; }

    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("isTorrentEnabled")]
    public bool IsTorrentEnabled { get; set; }

    [JsonPropertyName("magnetUri")]
    public string? MagnetUri { get; set; }

    [JsonPropertyName("viewCount")]
    public long ViewCount { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    public string GetIdString() => ExtractString(IdElement);

    public string GetTitleString() => ExtractString(TitleElement);

    public string GetUrlString() => ExtractString(UrlElement);

    public Video ToVideo()
    {
        var idStr = GetIdString();
        return new Video
        {
            Id = Guid.TryParse(idStr, out var guid) ? guid : Guid.Empty,
            Title = GetTitleString(),
            Name = GetTitleString(),
            Description = Description ?? string.Empty,
            Url = GetUrlString(),
            Thumbnail = ThumbnailUrl,
            Duration = Duration,
            CreatedAt = CreatedAt,
            IsTorrent = IsTorrentEnabled,
            MagnetUri = MagnetUri,
            ViewCount = ViewCount,
        };
    }

    private static string ExtractString(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString() ?? string.Empty;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("value", out var val) || element.TryGetProperty("Value", out val))
            {
                return val.GetString() ?? val.ToString();
            }
        }

        return element.ValueKind != JsonValueKind.Undefined && element.ValueKind != JsonValueKind.Null
            ? element.ToString()
            : string.Empty;
    }
}
