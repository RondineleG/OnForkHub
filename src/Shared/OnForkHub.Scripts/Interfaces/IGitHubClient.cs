namespace OnForkHub.Scripts.Interfaces;

public interface IGitHubClient
{
    Task EnsureLabelsExistAsync();
    Task<string?> FindExistingPullRequestAsync(string sourceBranch, string baseBranch);
    Task UpdatePullRequestAsync(string prNumber, PullRequestInfo prInfo);
    Task CreatePullRequestAsync(PullRequestInfo prInfo);
}