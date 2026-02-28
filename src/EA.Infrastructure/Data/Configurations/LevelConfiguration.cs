using EA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EA.Infrastructure.Data.Configurations;

public class LevelConfiguration : IEntityTypeConfiguration<Level>
{
    public void Configure(EntityTypeBuilder<Level> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Code).HasMaxLength(2).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();
        builder.HasIndex(l => l.Order).IsUnique();
    }
}
