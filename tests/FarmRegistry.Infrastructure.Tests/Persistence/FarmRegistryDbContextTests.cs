using FarmRegistry.Domain.Entities;
using FarmRegistry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FarmRegistry.Infrastructure.Tests.Persistence;

public class FarmRegistryDbContextTests
{
    private static readonly Guid OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly IReadOnlyCollection<FieldBoundaryCoordinate> Boundary =
    [
        new FieldBoundaryCoordinate(-21.2301, -47.8401),
        new FieldBoundaryCoordinate(-21.2297, -47.8397),
        new FieldBoundaryCoordinate(-21.2304, -47.8392)
    ];

    private static DbContextOptions<FarmRegistryDbContext> GetInMemoryDbOptions()
    {
        return new DbContextOptionsBuilder<FarmRegistryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void DbContext_ShouldInitialize_WithoutErrors()
    {
        var options = GetInMemoryDbOptions();

        using var context = new FarmRegistryDbContext(options);

        Assert.NotNull(context.Farms);
        Assert.NotNull(context.Fields);
        Assert.NotNull(context.FieldBoundaryPoints);
    }

    [Fact]
    public void DbContext_ShouldEnsureCreated_WithoutErrors()
    {
        var options = GetInMemoryDbOptions();

        using var context = new FarmRegistryDbContext(options);
        
        var result = context.Database.EnsureCreated();

        Assert.True(result);
    }

    [Fact]
    public void DbContext_ShouldSaveFarm_Successfully()
    {
        var options = GetInMemoryDbOptions();

        using var context = new FarmRegistryDbContext(options);
        context.Database.EnsureCreated();

        var farm = new Farm(OwnerId, "Fazenda Teste", "Ribeirão Preto", "SP");
        context.Farms.Add(farm);
        
        var result = context.SaveChanges();

        Assert.Equal(1, result);
        Assert.Single(context.Farms);
    }

    [Fact]
    public void DbContext_ShouldSaveFarmWithFields_Successfully()
    {
        var options = GetInMemoryDbOptions();

        using var context = new FarmRegistryDbContext(options);
        context.Database.EnsureCreated();

        var farm = new Farm(OwnerId, "Fazenda Teste", "Ribeirão Preto", "SP");
        var field = farm.AddField("T01", "Talhão 01", 10.5, "Milho", Boundary);
        
        context.Farms.Add(farm);
        
        var result = context.SaveChanges();

        Assert.True(result > 0);
        Assert.Single(context.Farms);
        Assert.Single(context.Fields);
        
        var savedField = context.Fields.First();
        Assert.Equal(farm.FarmId, savedField.FarmId);
        Assert.Equal("T01", savedField.Code);
        Assert.Equal("Talhão 01", savedField.Name);
        Assert.Equal(10.5, savedField.AreaHectares);
        Assert.Equal("Milho", savedField.CropName);
        Assert.Equal(3, context.FieldBoundaryPoints.Count());
    }

    [Fact]
    public void DbContext_ShouldUpdateAuditFields_WhenModifyingEntities()
    {
        var options = GetInMemoryDbOptions();

        using var context = new FarmRegistryDbContext(options);
        context.Database.EnsureCreated();

        var farm = new Farm(OwnerId, "Fazenda Teste", "Ribeirão Preto", "SP");
        context.Farms.Add(farm);
        context.SaveChanges();

        // Simular atualização
        farm.Update("Fazenda Atualizada", "São Paulo", "SP");
        context.Entry(farm).State = EntityState.Modified;
        
        context.SaveChanges();

        var updatedAt = context.Entry(farm).Property("UpdatedAt").CurrentValue;
        Assert.NotNull(updatedAt);
        Assert.IsType<DateTime>(updatedAt);
    }

    [Fact]
    public void DbContext_ShouldLoadFarmWithFields_UsingInclude()
    {
        var options = GetInMemoryDbOptions();

        using var context = new FarmRegistryDbContext(options);
        context.Database.EnsureCreated();

        // Arrange: Criar farm com múltiplos fields
        var farm = new Farm(OwnerId, "Fazenda com Talhões", "Ribeirão Preto", "SP");
        farm.AddField("T01", "Talhão 01", 10.5, "Milho", Boundary);
        farm.AddField("T02", "Talhão 02", 15.7, "Soja", Boundary);
        
        context.Farms.Add(farm);
        context.SaveChanges();
        context.ChangeTracker.Clear(); // Limpar tracking para simular nova consulta

        // Act: Carregar farm com fields usando Include
        var loadedFarm = context.Farms
            .Include(f => f.Fields)
            .First(f => f.FarmId == farm.FarmId);

        // Assert
        Assert.Equal("Fazenda com Talhões", loadedFarm.Name);
        Assert.Equal(2, loadedFarm.Fields.Count);
        Assert.Contains(loadedFarm.Fields, f => f.Code == "T01");
        Assert.Contains(loadedFarm.Fields, f => f.Code == "T02");
    }

    [Fact]
    public void DbContext_ShouldHandleMultipleFarms_WithIndependentFields()
    {
        var options = GetInMemoryDbOptions();

        using var context = new FarmRegistryDbContext(options);
        context.Database.EnsureCreated();

        // Criar duas fazendas com fields
        var farm1 = new Farm(OwnerId, "Fazenda A", "Ribeirão Preto", "SP");
        farm1.AddField("T01", "Talhão A1", 10.0, "Milho", Boundary);
        farm1.AddField("T02", "Talhão A2", 15.0, "Soja", Boundary);

        var farm2 = new Farm(Guid.NewGuid(), "Fazenda B", "Campinas", "SP");
        farm2.AddField("T01", "Talhão B1", 20.0, "Algodão", Boundary); // Mesmo código, mas farm diferente
        farm2.AddField("T03", "Talhão B3", 25.0, "Café", Boundary);

        context.Farms.AddRange(farm1, farm2);
        context.SaveChanges();

        // Verificar que cada farm tem seus próprios fields
        var farm1Fields = context.Fields.Where(f => f.FarmId == farm1.FarmId).ToList();
        var farm2Fields = context.Fields.Where(f => f.FarmId == farm2.FarmId).ToList();

        Assert.Equal(2, farm1Fields.Count);
        Assert.Equal(2, farm2Fields.Count);
        Assert.Equal(4, context.Fields.Count()); // Total de 4 fields

        // Verificar que ambas as farms podem ter field com código "T01"
        Assert.Contains(farm1Fields, f => f.Code == "T01");
        Assert.Contains(farm2Fields, f => f.Code == "T01");
    }

    [Fact]
    public void DbContext_ShouldPersistFieldStatusChanges()
    {
        var options = GetInMemoryDbOptions();

        using var context = new FarmRegistryDbContext(options);
        context.Database.EnsureCreated();

        var farm = new Farm(OwnerId, "Fazenda Teste", "Ribeirão Preto", "SP");
        var field = farm.AddField("T01", "Talhão 01", 10.5, "Milho", Boundary);
        
        context.Farms.Add(farm);
        context.SaveChanges();

        // Alterar status do field
        var savedFieldId = field.FieldId;
        var savedField = context.Fields.First(f => f.FieldId == savedFieldId);
        savedField.Deactivate(); // Muda para Inativo

        context.Entry(savedField).State = EntityState.Modified;
        context.SaveChanges();

        // Verificar mudança persistiu
        context.ChangeTracker.Clear();
        var reloadedField = context.Fields.First(f => f.FieldId == savedFieldId);
        
        Assert.Equal(FieldStatus.Inativo, reloadedField.Status);
        Assert.True(reloadedField.StatusUpdatedAt > field.CreatedAt);
    }

}
