namespace OnForkHub.Persistence.Contexts;

public class CosmosContainerDataContext<T>(Container container) : ICosmosContainerContext<T>
    where T : BaseEntity
{
    private readonly Container _container = container;

    public async Task<T> CreateAsync(T entity)
    {
        var response = await _container.CreateItemAsync(entity);
        return response.Resource;
    }

    public async Task<bool> DeleteAsync(string id, string partitionKey)
    {
        var response = await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public async Task<IEnumerable<T>> GetAsync(string queryString, QueryRequestOptions? requestOptions = null)
    {
        var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString), null, requestOptions);
        var results = new List<T>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<T> GetByIdAsync(string id, string partitionKey)
    {
        var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));

        try
        {
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return response.Resource;
        }
    }

    public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, string? continuationToken = null)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c ORDER BY c._ts DESC");
        var queryRequestOptions = new QueryRequestOptions { MaxItemCount = pageSize };

        var startFrom = (pageNumber - 1) * pageSize;
        var query = _container.GetItemQueryIterator<T>(queryDefinition, continuationToken, queryRequestOptions);

        var results = new List<T>();
        var currentCount = 0;

        while (query.HasMoreResults && currentCount < pageSize)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
            currentCount += response.Count;
        }

        return results.Skip(startFrom).Take(pageSize);
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var response = await _container.UpsertItemAsync(entity);
        return response.Resource;
    }
}
