namespace OnForkHub.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnForkHub.Core.Entities;

/// <summary>
/// EF Core configuration for Category entity with optimized indexes.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        // Configure Name value object
        builder.OwnsOne(
            c => c.Name,
            name =>
            {
                name.Property(n => n.Value).HasColumnName("Name").HasMaxLength(100).IsRequired();

                // Index on Name for search queries and sorting
                name.HasIndex(n => n.Value).HasDatabaseName("IX_Categories_Name");
            }
        );

        builder.Property(c => c.Description).HasMaxLength(500);

        // Index on CreatedAt for sorting
        builder.HasIndex(c => c.CreatedAt).HasDatabaseName("IX_Categories_CreatedAt");
    }
}
