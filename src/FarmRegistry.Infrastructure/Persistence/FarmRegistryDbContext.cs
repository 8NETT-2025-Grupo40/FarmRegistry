using FarmRegistry.Domain.Entities;
using FarmRegistry.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FarmRegistry.Infrastructure.Persistence;

public class FarmRegistryDbContext : DbContext
{
    public FarmRegistryDbContext(DbContextOptions<FarmRegistryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Farm> Farms { get; set; } = null!;
    public DbSet<Field> Fields { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new FarmEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FieldEntityTypeConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Configurar para não usar constructor binding
        optionsBuilder.EnableServiceProviderCaching(false);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
            if (updatedAtProperty != null)
            {
                updatedAtProperty.CurrentValue = DateTime.UtcNow;
            }
        }
    }
}