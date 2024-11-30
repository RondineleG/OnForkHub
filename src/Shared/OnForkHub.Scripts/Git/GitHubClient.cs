using OnForkHub.Scripts.Enums;
using OnForkHub.Scripts.Interfaces;

namespace OnForkHub.Scripts.Git;

public class GitHubClient(IProcessRunner processRunner, ILogger logger) : IGitHubClient
{
    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Dictionary<string, string> _requiredLabels =
        new()
        {
            { "in-review", "#19034f" },
            { "high", "#7a2102" },
            { "large", "#010821" },
        };

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

    public async Task UpdatePullRequestAsync(string prNumber, PullRequestInfo prInfo)
    {
        var editCommand = BuildPullRequestCommand("pr edit", prNumber, prInfo);
        await _processRunner.RunAsync("gh", editCommand);
    }

    public async Task CreatePullRequestAsync(PullRequestInfo prInfo)
    {
        var createCommand = BuildPullRequestCommand("pr create", null, prInfo);
        await _processRunner.RunAsync("gh", createCommand);
    }

    private static string BuildPullRequestCommand(string action, string? prNumber, PullRequestInfo prInfo)
    {
        var command = new List<string>
        {
            prNumber == null ? action : $"{action} {prNumber}",
            $"--title \"{prInfo.Title}\"",
            $"--body \"{prInfo.Body}\"",
        };

        if (action == "pr create")
        {
            command.Add($"--base {prInfo.BaseBranch}");
            command.Add($"--head {prInfo.SourceBranch}");
        }

        command.AddRange(["--label \"status:in-review,priority:high,size:large\"", "--assignee @me", "--milestone onforkhub-core-foundation"]);

        return string.Join(" ", command);
    }
}
