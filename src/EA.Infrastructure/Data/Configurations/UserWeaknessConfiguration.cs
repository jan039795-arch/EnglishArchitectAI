using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class UserWeaknessConfiguration : IEntityTypeConfiguration<UserWeakness>
{
    public void Configure(EntityTypeBuilder<UserWeakness> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Tag).HasMaxLength(100).IsRequired();

        builder.HasIndex(w => new { w.UserId, w.Tag }).IsUnique();

        builder.HasOne(w => w.User)
            .WithMany(u => u.Weaknesses)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
