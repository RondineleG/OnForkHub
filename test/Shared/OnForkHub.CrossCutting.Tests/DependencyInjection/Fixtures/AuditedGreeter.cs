namespace OnForkHub.CrossCutting.Tests.DependencyInjection.Fixtures;

public sealed class AuditedGreeter : IScanGreeter, IScanAudit
{
    public string Greet() => "audited";
}
