namespace OnForkHub.Persistence.Contexts.Base;

public interface ICosmosDbContext : IAsyncDisposable
{
    Container GetContainer(string containerName);
    Task<Container> CreateContainerIfNotExistsAsync(string containerName, string partitionKeyPath);
    Task InitializeDatabaseAsync();
}