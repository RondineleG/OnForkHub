namespace OnForkHub.Scripts.Interfaces;

public interface IGitHubClient
{
    Task CreatePullRequestAsync(PullRequestInfo prInfo);

    Task EnsureLabelsExistAsync();

    Task<string?> FindExistingPullRequestAsync(string sourceBranch, string baseBranch);

    Task UpdatePullRequestAsync(string prNumber, PullRequestInfo prInfo);
}
