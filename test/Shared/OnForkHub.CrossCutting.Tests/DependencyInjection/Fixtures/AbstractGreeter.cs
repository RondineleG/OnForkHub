namespace OnForkHub.CrossCutting.Tests.DependencyInjection.Fixtures;

public abstract class AbstractGreeter : IScanGreeter
{
    public abstract string Greet();
}
