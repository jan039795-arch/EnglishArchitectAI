using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class SpacedRepetitionCardConfiguration : IEntityTypeConfiguration<SpacedRepetitionCard>
{
    public void Configure(EntityTypeBuilder<SpacedRepetitionCard> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => new { c.UserId, c.NextReviewDate });

        builder.HasOne(c => c.User)
            .WithMany(u => u.SpacedRepetitionCards)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Exercise)
            .WithMany(e => e.SpacedRepetitionCards)
            .HasForeignKey(c => c.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
