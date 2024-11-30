namespace OnForkHub.Persistence.Contexts;

public sealed class EntityFrameworkDataContext(DbContextOptions<EntityFrameworkDataContext> options) : DbContext(options), IEntityFrameworkDataContext
{
    public DbSet<Category> Categories { get; set; } = null!;

    EntityEntry<TEntity> IEntityFrameworkDataContext.Entry<TEntity>(TEntity entity)
    {
        return Entry(entity);
    }
}
