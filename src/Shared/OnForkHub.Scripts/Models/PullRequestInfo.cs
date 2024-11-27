namespace OnForkHub.Scripts.Models;

public class PullRequestInfo(string title, string body, string baseBranch, string sourceBranch)
{
    public string Title { get; set; } = title;
    public string Body { get; set; } = body;

    public string BaseBranch { get; set; } = baseBranch;

    public string SourceBranch { get; set; } = sourceBranch;
}
