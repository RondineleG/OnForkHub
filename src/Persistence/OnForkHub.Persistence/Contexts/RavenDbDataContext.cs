namespace OnForkHub.Persistence.Contexts;

public class RavenDbDataContext(IDocumentStore documentStore)
{
    public IDocumentStore Store { get; } = documentStore;
}