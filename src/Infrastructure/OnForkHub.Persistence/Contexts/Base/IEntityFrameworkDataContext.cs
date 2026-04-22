namespace OnForkHub.Persistence.Contexts.Base;

public interface IEntityFrameworkDataContext
{
    DbSet<Category> Categories { get; set; }

    DbSet<Notification> Notifications { get; set; }

    DbSet<RefreshToken> RefreshTokens { get; set; }

    DbSet<UserEntity> Users { get; set; }

    DbSet<Video> Videos { get; set; }

    DbSet<VideoUpload> VideoUploads { get; set; }

    DbSet<Comment> Comments { get; set; }

    DbSet<VideoRating> VideoRatings { get; set; }

    DbSet<UserFavorite> UserFavorites { get; set; }

    DbSet<ViewHistory> ViewHistories { get; set; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
