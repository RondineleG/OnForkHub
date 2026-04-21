namespace OnForkHub.Persistence.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnForkHub.Core.Entities;

/// <summary>
/// Configuration for VideoUpload entity.
/// </summary>
public sealed class VideoUploadConfiguration : IEntityTypeConfiguration<VideoUpload>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<VideoUpload> builder)
    {
        builder.ToTable("VideoUploads");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.ToString(), x => x)
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.StoragePath)
            .HasMaxLength(500);

        builder.Property(x => x.ProgressPercentage)
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasConversion(x => x.ToString(), x => x)
            .IsRequired();

        builder.Property(x => x.TotalChunks)
            .IsRequired();

        builder.Property(x => x.ReceivedChunks)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.CompletedAt);

        // Indexes for performance
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);
    }
}
