using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(1000);
        builder.Property(m => m.YoutubePlaylistId).HasMaxLength(100);

        builder.HasOne(m => m.Level)
            .WithMany(l => l.Modules)
            .HasForeignKey(m => m.LevelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
