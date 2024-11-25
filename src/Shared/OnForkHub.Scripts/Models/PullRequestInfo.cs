namespace OnForkHub.Scripts.Models;

public class PullRequestInfo(string sourceBranch)
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public string BaseBranch { get; set; } = string.Empty;

    public string SourceBranch { get; set; } = sourceBranch;
}
