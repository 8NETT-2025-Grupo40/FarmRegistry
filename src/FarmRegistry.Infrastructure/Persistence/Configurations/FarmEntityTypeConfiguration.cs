using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Infrastructure.Persistence.Configurations;

public class FarmEntityTypeConfiguration : IEntityTypeConfiguration<Farm>
{
    public void Configure(EntityTypeBuilder<Farm> builder)
    {
        builder.ToTable("Farms");

        // Configurar chave primária
        builder.HasKey(f => f.FarmId);
        
        builder.Property(f => f.FarmId)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(f => f.OwnerId)
            .IsRequired();

        // Configurar propriedades
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.State)
            .IsRequired()
            .HasMaxLength(2)
            .IsFixedLength();

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Shadow Properties para auditoria
        builder.Property<DateTime?>("UpdatedAt");
        builder.Property<string>("UpdatedBy")
            .HasMaxLength(100);

        builder.HasMany(f => f.Fields)
            .WithOne()
            .HasForeignKey(field => field.FarmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Fields)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Índices para performance
        builder.HasIndex(f => f.Name);
        builder.HasIndex(f => f.IsActive);
        builder.HasIndex(f => f.OwnerId);
        builder.HasIndex(f => new { f.City, f.State });
    }
}
