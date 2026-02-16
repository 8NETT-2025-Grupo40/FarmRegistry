using FarmRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmRegistry.Infrastructure.Persistence.Configurations;

public class FieldBoundaryPointEntityTypeConfiguration : IEntityTypeConfiguration<FieldBoundaryPoint>
{
    public void Configure(EntityTypeBuilder<FieldBoundaryPoint> builder)
    {
        builder.ToTable("FieldBoundaryPoints");

        builder.HasKey(point => point.FieldBoundaryPointId);

        builder.Property(point => point.FieldBoundaryPointId)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(point => point.FieldId)
            .IsRequired();

        builder.Property(point => point.Sequence)
            .IsRequired();

        builder.Property(point => point.Latitude)
            .IsRequired();

        builder.Property(point => point.Longitude)
            .IsRequired();

        builder.HasIndex(point => point.FieldId);
        builder.HasIndex(point => new { point.FieldId, point.Sequence })
            .IsUnique();
    }
}
