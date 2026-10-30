namespace OnForkHub.CrossCutting.Tests.DependencyInjection.Fixtures;

public sealed class EnglishGreeter : IScanGreeter, IDisposable
{
    public void Dispose() { }

    public string Greet() => "hello";
}
