// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Contexts.Base;

public interface IEntityFrameworkDataContext
{
    DbSet<Category> Categories { get; set; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
