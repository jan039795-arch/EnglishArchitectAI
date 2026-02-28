using EA.Domain.Entities;
using EA.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.CEFRLevel)
            .HasConversion<string>()
            .HasMaxLength(2);

        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}
