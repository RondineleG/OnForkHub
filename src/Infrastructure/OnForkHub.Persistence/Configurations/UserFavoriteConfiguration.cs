namespace OnForkHub.Persistence.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnForkHub.Core.Entities;

/// <summary>
/// EF Core configuration for UserFavorite entity.
/// </summary>
public sealed class UserFavoriteConfiguration : IEntityTypeConfiguration<UserFavorite>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserFavorite> builder)
    {
        builder.ToTable("UserFavorites");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(x => x.ToString(), x => x).IsRequired();

        builder.Property(x => x.VideoId).IsRequired();

        builder.Property(x => x.UserId).HasConversion(x => x.ToString(), x => x).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Unique index to ensure one favorite per user per video
        builder.HasIndex(x => new { x.VideoId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
