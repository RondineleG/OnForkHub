namespace OnForkHub.Persistence.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnForkHub.Core.Entities;

/// <summary>
/// EF Core configuration for ViewHistory entity.
/// </summary>
public sealed class ViewHistoryConfiguration : IEntityTypeConfiguration<ViewHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ViewHistory> builder)
    {
        builder.ToTable("ViewHistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(x => x.ToString(), x => x).IsRequired();

        builder.Property(x => x.VideoId).IsRequired();

        builder.Property(x => x.UserId).HasConversion(x => x.ToString(), x => x).IsRequired();

        builder.Property(x => x.LastViewedAt).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Index for performance
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.VideoId });
    }
}
