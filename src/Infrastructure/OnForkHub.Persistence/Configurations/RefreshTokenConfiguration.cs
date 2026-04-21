namespace OnForkHub.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnForkHub.Core.Entities;

/// <summary>
/// Entity configuration for RefreshToken.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id).HasColumnName("Id").HasMaxLength(100).IsRequired();

        // Token value - indexed for fast lookups
        builder.Property(rt => rt.Token).HasColumnName("Token").HasMaxLength(500).IsRequired();

        builder.HasIndex(rt => rt.Token).HasDatabaseName("IX_RefreshTokens_Token").IsUnique();

        // User relationship
        builder.Property(rt => rt.UserId).HasColumnName("UserId").HasMaxLength(36).IsRequired();

        builder.HasIndex(rt => rt.UserId).HasDatabaseName("IX_RefreshTokens_UserId");

        // Composite index for user's active tokens
        builder.HasIndex(rt => new { rt.UserId, rt.RevokedAt }).HasDatabaseName("IX_RefreshTokens_UserId_RevokedAt");

        // Expiration
        builder.Property(rt => rt.ExpiresAt).HasColumnName("ExpiresAt").IsRequired();

        builder.HasIndex(rt => rt.ExpiresAt).HasDatabaseName("IX_RefreshTokens_ExpiresAt");

        // Revoked timestamp (nullable)
        builder.Property(rt => rt.RevokedAt).HasColumnName("RevokedAt").IsRequired(false);

        // CreatedByIp
        builder
            .Property(rt => rt.CreatedByIp)
            .HasColumnName("CreatedByIp")
            .HasMaxLength(45) // IPv6 max length
            .IsRequired(false);

        // UserAgent
        builder.Property(rt => rt.UserAgent).HasColumnName("UserAgent").HasMaxLength(500).IsRequired(false);

        // Base entity properties
        builder.Property(rt => rt.CreatedAt).HasColumnName("CreatedAt").IsRequired();

        builder.Property(rt => rt.UpdatedAt).HasColumnName("UpdatedAt").IsRequired(false);
    }
}
