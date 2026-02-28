using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class ExerciseOptionConfiguration : IEntityTypeConfiguration<ExerciseOption>
{
    public void Configure(EntityTypeBuilder<ExerciseOption> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Text).HasMaxLength(500).IsRequired();
        builder.Property(o => o.Explanation).HasMaxLength(1000);

        builder.HasOne(o => o.Exercise)
            .WithMany(e => e.Options)
            .HasForeignKey(o => o.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
