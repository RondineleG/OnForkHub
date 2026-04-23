namespace OnForkHub.Persistence.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnForkHub.Core.Entities;

/// <summary>
/// EF Core configuration for VideoRating entity.
/// </summary>
public sealed class VideoRatingConfiguration : IEntityTypeConfiguration<VideoRating>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<VideoRating> builder)
    {
        builder.ToTable("VideoRatings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(x => x.ToString(), x => x).IsRequired();

        builder.Property(x => x.VideoId).IsRequired();

        builder.Property(x => x.UserId).HasConversion(x => x.ToString(), x => x).IsRequired();

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Unique index to ensure one rating per user per video
        builder.HasIndex(x => new { x.VideoId, x.UserId }).IsUnique();
    }
}
