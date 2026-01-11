using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnForkHub.Persistence.Contexts.Mappings.Identity;

public class IdentityUserTokenMapping : IEntityTypeConfiguration<IdentityUserToken<long>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<long>> builder)
    {
        builder.ToTable("UserTokens");
        builder.HasKey(t => new
        {
            t.UserId,
            t.LoginProvider,
            t.Name,
        });
        builder.Property(t => t.LoginProvider).HasMaxLength(120);
        builder.Property(t => t.Name).HasMaxLength(180);
    }
}
