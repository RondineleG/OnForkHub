// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Contexts;

public class RavenDbDataContext(IDocumentStore documentStore)
{
    public IDocumentStore Store { get; } = documentStore;
}
