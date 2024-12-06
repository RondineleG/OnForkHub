namespace OnForkHub.Persistence.Contexts.Base;

public interface IEntityFrameworkDataContext
{
    DbSet<Category> Categories { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;
}
