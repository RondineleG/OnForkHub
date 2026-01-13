namespace OnForkHub.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnForkHub.Core.Entities;

/// <summary>
/// EF Core configuration for Video entity with optimized indexes.
/// </summary>
public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("Videos");

        builder.HasKey(v => v.Id);

        // Configure Title value object
        builder.OwnsOne(
            v => v.Title,
            title =>
            {
                title.Property(t => t.Value).HasColumnName("Title").HasMaxLength(200).IsRequired();

                // Index on Title for search queries
                title.HasIndex(t => t.Value).HasDatabaseName("IX_Videos_Title");
            }
        );

        // Configure UserId value object
        builder.OwnsOne(
            v => v.UserId,
            userId =>
            {
                userId.Property(u => u.Value).HasColumnName("UserId").HasMaxLength(36);

                // Index on UserId for filtering by user
                userId.HasIndex(u => u.Value).HasDatabaseName("IX_Videos_UserId");
            }
        );

        builder.Property(v => v.Description).HasMaxLength(2000);

        builder.Property(v => v.Url).HasMaxLength(500).IsRequired();

        // Index on CreatedAt for sorting and date range filtering
        builder.HasIndex(v => v.CreatedAt).HasDatabaseName("IX_Videos_CreatedAt");

        // Index on UpdatedAt for sorting
        builder.HasIndex(v => v.UpdatedAt).HasDatabaseName("IX_Videos_UpdatedAt");

        // Configure relationship with Categories
        builder.HasMany(v => v.Categories).WithMany().UsingEntity(j => j.ToTable("VideoCategories"));
    }
}
