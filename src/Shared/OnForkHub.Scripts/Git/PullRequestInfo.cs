namespace OnForkHub.Scripts.Git;

public class PullRequestInfo
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string BaseBranch { get; set; } = string.Empty;
    public string SourceBranch { get; set; } = string.Empty;
}
