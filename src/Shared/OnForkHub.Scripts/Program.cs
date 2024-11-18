using OnForkHub.Scripts;

if (!await GitFlowConfiguration.VerifyGitInstallationAsync())
{
    return;
}

await GitFlowConfiguration.ApplySharedConfigurationsAsync();
