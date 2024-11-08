var gitConfigurator = new GitConfiguration();

if (!await gitConfigurator.VerifyGitInstallationAsync())
{
    return;
}
await gitConfigurator.ApplySharedConfigurationsAsync();
