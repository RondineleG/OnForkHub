namespace OnForkHub.Scripts.Interfaces;

public interface IGitHubClient
{
    Task CreatePullRequestAsync(PullRequestInfo pullRequestInfo);

    Task EnsureLabelsExistAsync();

    Task<string?> FindExistingPullRequestAsync(string sourceBranch, string baseBranch);

    Task UpdatePullRequestAsync(string pullRequestNumber, PullRequestInfo pullRequestInfo);
}
