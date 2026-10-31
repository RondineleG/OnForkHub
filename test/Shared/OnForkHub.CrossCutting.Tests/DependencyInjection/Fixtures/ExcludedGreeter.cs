using OnForkHub.Core.Attributes;

namespace OnForkHub.CrossCutting.Tests.DependencyInjection.Fixtures;

[ExcludeFromRegistration]
public sealed class ExcludedGreeter : IScanGreeter
{
    public string Greet() => "excluded";
}
