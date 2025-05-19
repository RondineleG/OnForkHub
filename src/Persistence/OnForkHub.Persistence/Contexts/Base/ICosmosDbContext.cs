// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Contexts.Base;

public interface ICosmosDbContext : IAsyncDisposable
{
    Task<Container> CreateContainerIfNotExistsAsync(string containerName, string partitionKeyPath);

    Container GetContainer(string containerName);

    Task InitializeDatabaseAsync();
}
