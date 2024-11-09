namespace OnForkHub.Persistence.Contexts;

public sealed class EntityFrameworkDataContext(DbContextOptions<EntityFrameworkDataContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
}
