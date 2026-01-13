namespace OnForkHub.Persistence.Contexts.Base;

public interface IEntityFrameworkDataContext
{
    DbSet<Category> Categories { get; set; }

    DbSet<Notification> Notifications { get; set; }

    DbSet<Video> Videos { get; set; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
