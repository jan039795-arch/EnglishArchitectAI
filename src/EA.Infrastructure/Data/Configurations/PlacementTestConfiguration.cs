using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class PlacementTestConfiguration : IEntityTypeConfiguration<PlacementTest>
{
    public void Configure(EntityTypeBuilder<PlacementTest> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FinalLevel).HasConversion<string>().HasMaxLength(2);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
