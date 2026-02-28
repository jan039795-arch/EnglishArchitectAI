using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class SpeakingAttemptConfiguration : IEntityTypeConfiguration<SpeakingAttempt>
{
    public void Configure(EntityTypeBuilder<SpeakingAttempt> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.AudioBlobUrl).HasMaxLength(500);

        builder.HasOne(s => s.User)
            .WithMany(u => u.SpeakingAttempts)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Exercise)
            .WithMany()
            .HasForeignKey(s => s.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
