namespace OnForkHub.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Notification entity with optimized indexes.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        // Configure Title value object
        builder.OwnsOne(
            n => n.Title,
            title =>
            {
                title.Property(t => t.Value).HasColumnName("Title").HasMaxLength(200).IsRequired();
            }
        );

        // Configure UserId value object
        builder.OwnsOne(
            n => n.UserId,
            userId =>
            {
                userId.Property(u => u.Value).HasColumnName("UserId").HasMaxLength(36).IsRequired();
                userId.HasIndex(u => u.Value).HasDatabaseName("IX_Notifications_UserId");
            }
        );

        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.ReferenceId).HasMaxLength(100);

        // Indexes for common query patterns
        builder.HasIndex(n => n.CreatedAt).HasDatabaseName("IX_Notifications_CreatedAt");
        builder.HasIndex(n => n.Status).HasDatabaseName("IX_Notifications_Status");
        builder.HasIndex(n => n.Type).HasDatabaseName("IX_Notifications_Type");
        builder.HasIndex(n => n.ReadAt).HasDatabaseName("IX_Notifications_ReadAt");
    }
}
