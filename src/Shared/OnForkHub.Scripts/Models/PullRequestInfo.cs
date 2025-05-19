// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Scripts.Models;

public record PullRequestInfo(string Title, string Body, string BaseBranch, string SourceBranch);
