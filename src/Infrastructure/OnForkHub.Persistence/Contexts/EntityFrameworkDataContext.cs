namespace OnForkHub.Persistence.Contexts;

using OnForkHub.Persistence.Configurations;

public sealed class EntityFrameworkDataContext(DbContextOptions<EntityFrameworkDataContext> options) : DbContext(options), IEntityFrameworkDataContext
{
    public DbSet<Category> Categories { get; set; } = null!;

    public DbSet<Notification> Notifications { get; set; } = null!;

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    public DbSet<UserEntity> Users { get; set; } = null!;

    public DbSet<Video> Videos { get; set; } = null!;

    public DbSet<VideoUpload> VideoUploads { get; set; } = null!;

    public DbSet<Comment> Comments { get; set; } = null!;

    public DbSet<VideoRating> VideoRatings { get; set; } = null!;

    public DbSet<UserFavorite> UserFavorites { get; set; } = null!;

    public DbSet<ViewHistory> ViewHistories { get; set; } = null!;

    EntityEntry<TEntity> IEntityFrameworkDataContext.Entry<TEntity>(TEntity entity)
    {
        return Entry(entity);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations with optimized indexes
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new VideoConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new VideoUploadConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
        modelBuilder.ApplyConfiguration(new VideoRatingConfiguration());
        modelBuilder.ApplyConfiguration(new UserFavoriteConfiguration());
        modelBuilder.ApplyConfiguration(new ViewHistoryConfiguration());
    }
}
