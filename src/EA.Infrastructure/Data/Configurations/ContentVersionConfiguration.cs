using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class ContentVersionConfiguration : IEntityTypeConfiguration<ContentVersion>
{
    public void Configure(EntityTypeBuilder<ContentVersion> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.OriginalPrompt).HasMaxLength(1000).IsRequired();
        builder.Property(c => c.GeneratedPrompt).HasMaxLength(1000).IsRequired();
        builder.Property(c => c.ChangeLog).HasMaxLength(500);
        builder.Property(c => c.VersionNumber).IsRequired();
        builder.Property(c => c.IsActive).IsRequired();

        builder.HasOne(c => c.Exercise)
            .WithMany()
            .HasForeignKey(c => c.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ExerciseId);
        builder.HasIndex(c => new { c.ExerciseId, c.VersionNumber }).IsUnique();
    }
}
