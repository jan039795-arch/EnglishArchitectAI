using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Source).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Prompt).IsRequired();
        builder.Property(e => e.CorrectAnswer).IsRequired();
        builder.Property(e => e.CacheKey).HasMaxLength(200);
        builder.Property(e => e.Tags).HasMaxLength(500);

        builder.HasOne(e => e.Lesson)
            .WithMany(l => l.Exercises)
            .HasForeignKey(e => e.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
