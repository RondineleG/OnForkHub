using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnForkHub.Persistence.Contexts.Mappings.Identity;

public class IdentityRoleMapping : IEntityTypeConfiguration<IdentityRole<long>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<long>> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.NormalizedName).IsUnique();
        builder.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();
        builder.Property(u => u.Name).HasMaxLength(256);
        builder.Property(u => u.NormalizedName).HasMaxLength(256);
    }
}
