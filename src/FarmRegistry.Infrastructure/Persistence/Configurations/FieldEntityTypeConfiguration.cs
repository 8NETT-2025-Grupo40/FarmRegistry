using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Infrastructure.Persistence.Configurations;

public class FieldEntityTypeConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        builder.ToTable("Fields");

        // Configurar chave primária
        builder.HasKey(f => f.FieldId);
        
        builder.Property(f => f.FieldId)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(f => f.FarmId)
            .IsRequired();

        builder.Property(f => f.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.AreaHectares)
            .IsRequired();

        builder.Property(f => f.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(FieldStatus.Normal);

        builder.Property(f => f.StatusUpdatedAt)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // **FIX: Remover a configuração de navegação Farm se não for necessária**
        // Se você quiser navegação bidirecional, configure aqui:
        // builder.HasOne<Farm>()
        //     .WithMany(f => f.Fields)
        //     .HasForeignKey(f => f.FarmId);

        // Shadow Properties para auditoria
        builder.Property<DateTime?>("UpdatedAt");
        builder.Property<string>("UpdatedBy")
            .HasMaxLength(100);

        // Índices para performance e regras de negócio
        builder.HasIndex(f => f.FarmId);
        builder.HasIndex(f => f.Code);
        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => new { f.FarmId, f.Code })
            .IsUnique();
    }
}