namespace OnForkHub.Persistence.Configurations;

public class RavenDbSettings
{
    public string[] Urls { get; set; } = [];
    public string Database { get; set; } = string.Empty;
}
