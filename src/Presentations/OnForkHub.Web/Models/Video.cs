namespace OnForkHub.Web.Models;

public class Video
{
    [Required]
    public string? Thumbnail { get; set; } = string.Empty;

    [Required]
    public string? Title { get; set; } = string.Empty;

    [Required]
    public string? Name { get; set; } = string.Empty;
}
