namespace OnForkHub.Persistence.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnForkHub.Core.Entities;

/// <summary>
/// EF Core configuration for Comment entity.
/// </summary>
public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(x => x.ToString(), x => x).IsRequired();

        builder.Property(x => x.Content).HasMaxLength(1000).IsRequired();

        builder.Property(x => x.VideoId).IsRequired();

        builder.Property(x => x.UserId).HasConversion(x => x.ToString(), x => x).IsRequired();

        builder.Property(x => x.ParentCommentId);

        builder.Property(x => x.IsEdited).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Indexes
        builder.HasIndex(x => x.VideoId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ParentCommentId);
    }
}
