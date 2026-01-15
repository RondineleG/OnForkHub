namespace OnForkHub.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OnForkHub.Core.Entities;

/// <summary>
/// EF Core configuration for User entity with optimized indexes.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        // Configure Email value object
        builder.OwnsOne(
            u => u.Email,
            email =>
            {
                email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(256).IsRequired();

                // Unique index on Email for login and duplicate prevention
                email.HasIndex(e => e.Value).IsUnique().HasDatabaseName("IX_Users_Email");
            }
        );

        // Configure Name value object
        builder.OwnsOne(
            u => u.Name,
            name =>
            {
                name.Property(n => n.Value).HasColumnName("Name").HasMaxLength(100).IsRequired();
            }
        );

        // PasswordHash property
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();

        // Index on CreatedAt for sorting
        builder.HasIndex(u => u.CreatedAt).HasDatabaseName("IX_Users_CreatedAt");

        // Configure relationship with Videos
        builder.HasMany(u => u.Videos).WithOne().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade);
    }
}
