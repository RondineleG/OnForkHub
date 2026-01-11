namespace OnForkHub.Scripts.Git;

using OnForkHub.Scripts.Interfaces;

public class GitHubClient(IProcessRunner processRunner, ILogger logger) : IGitHubClient
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    private readonly Dictionary<string, string> _requiredLabels = new()
    {
        { "in-review", "#19034f" },
        { "high", "#7a2102" },
        { "large", "#010821" },
    };

    public async Task CreatePullRequestAsync(PullRequestInfo pullRequestInfo)
    {
        var createCommand = BuildPullRequestCommand("pr create", null, pullRequestInfo);
        await _processRunner.RunAsync("gh", createCommand);
    }

    public async Task EnsureLabelsExistAsync()
    {
        foreach (var label in _requiredLabels)
        {
            try
            {
                await _processRunner.RunAsync("gh", $"label list --search \"{label.Key}\"");
                var result = await _processRunner.RunAsync("gh", $"label create \"{label.Key}\" --color \"{label.Value}\" --force");
                _logger.Log(ELogLevel.Info, $"Label check/creation: {result}");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Label creation warning: {ex.Message}");
            }
        }
    }

    public async Task<string?> FindExistingPullRequestAsync(string sourceBranch, string baseBranch)
    {
        var existingPRs = await _processRunner.RunAsync("gh", $"pr list --head {sourceBranch} --base {baseBranch} --state open");

        return string.IsNullOrWhiteSpace(existingPRs) ? null : existingPRs.Split('\t')[0];
    }

    public async Task UpdatePullRequestAsync(string pullRequestNumber, PullRequestInfo pullRequestInfo)
    {
        var editCommand = BuildPullRequestCommand("pr edit", pullRequestNumber, pullRequestInfo);
        await _processRunner.RunAsync("gh", editCommand);
    }

    private static string BuildPullRequestCommand(string action, string? pullRequestNumber, PullRequestInfo pullRequestInfo)
    {
        var command = new List<string>
        {
            pullRequestNumber == null ? action : $"{action} {pullRequestNumber}",
            $"--title \"{pullRequestInfo.Title}\"",
            $"--body \"{pullRequestInfo.Body}\"",
        };

        if (action == "pr create")
        {
            command.Add($"--base {pullRequestInfo.BaseBranch}");
            command.Add($"--head {pullRequestInfo.SourceBranch}");
        }

        command.AddRange(["--label \"status:in-review,priority:high,size:large\"", "--assignee @me", "--milestone onforkhub-core-foundation"]);

        return string.Join(" ", command);
    }
}
