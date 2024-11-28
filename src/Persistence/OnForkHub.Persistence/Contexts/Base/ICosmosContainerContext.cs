using OnForkHub.Core.Entities.Base;

namespace OnForkHub.Persistence.Contexts.Base;

public interface ICosmosContainerContext<T>
    where T : BaseEntity
{
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id, string partitionKey);
    Task<T> GetByIdAsync(string id, string partitionKey);
    Task<IEnumerable<T>> GetAsync(string queryString, QueryRequestOptions? requestOptions = null);
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, string? continuationToken = null);
}
