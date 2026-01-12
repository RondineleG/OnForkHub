using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnForkHub.Persistence.Contexts.Mappings.Identity;

public class IdentityUserClaimMapping : IEntityTypeConfiguration<IdentityUserClaim<long>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<long>> builder)
    {
        builder.ToTable("UserClaims");
        builder.HasKey(uc => uc.Id);
        builder.Property(u => u.ClaimType).HasMaxLength(255);
        builder.Property(u => u.ClaimValue).HasMaxLength(255);
    }
}
