using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class LessonCommentConfiguration : IEntityTypeConfiguration<LessonComment>
{
    public void Configure(EntityTypeBuilder<LessonComment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Body).HasMaxLength(500).IsRequired();
        builder.Property(c => c.UserId).IsRequired();

        builder.HasOne(c => c.Lesson)
            .WithMany(l => l.Comments)
            .HasForeignKey(c => c.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
