namespace OnForkHub.Persistence.Contexts;

using OnForkHub.Persistence.Configurations;

public sealed class EntityFrameworkDataContext(DbContextOptions<EntityFrameworkDataContext> options) : DbContext(options), IEntityFrameworkDataContext
{
    public DbSet<Category> Categories { get; set; } = null!;

    public DbSet<Notification> Notifications { get; set; } = null!;

    public DbSet<Video> Videos { get; set; } = null!;

    EntityEntry<TEntity> IEntityFrameworkDataContext.Entry<TEntity>(TEntity entity)
    {
        return Entry(entity);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations with optimized indexes
        modelBuilder.ApplyConfiguration(new VideoConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
    }
}
