using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Domain.Tests;

public class FarmTests
{
    [Fact]
    public void Create_Farm_ShouldStartActive_AndHaveValidData()
    {
        var farm = new Farm("Fazenda Boa Vista", "Ribeirão Preto", "SP");

        Assert.NotEqual(Guid.Empty, farm.FarmId);
        Assert.True(farm.IsActive);
        Assert.Equal("Fazenda Boa Vista", farm.Name);
        Assert.Equal("Ribeirão Preto", farm.City);
        Assert.Equal("SP", farm.State);
        Assert.True(farm.CreatedAt != default);
        Assert.Empty(farm.Fields);
    }

    [Fact]
    public void Update_Farm_ShouldChangeNameCityState()
    {
        var farm = new Farm("Fazenda A", "Campinas", "SP");

        farm.Update("Fazenda B", "Sorocaba", "sp");

        Assert.Equal("Fazenda B", farm.Name);
        Assert.Equal("Sorocaba", farm.City);
        Assert.Equal("SP", farm.State);
    }

    [Fact]
    public void ActivateDeactivate_Farm_ShouldToggleIsActive()
    {
        var farm = new Farm("Fazenda A", "Campinas", "SP");

        farm.Deactivate();
        Assert.False(farm.IsActive);

        farm.Activate();
        Assert.True(farm.IsActive);
    }

    [Fact]
    public void AddField_ShouldCreateFieldLinkedToFarm()
    {
        var farm = new Farm("Fazenda A", "Campinas", "SP");

        var field = farm.AddField("T01", "Talhão 01", 10);

        Assert.Single(farm.Fields);
        Assert.Equal(farm.FarmId, field.FarmId);
        Assert.Equal("T01", field.Code);
        Assert.Equal("Talhão 01", field.Name);
        Assert.Equal(10, field.AreaHectares);
        Assert.Equal(FieldStatus.Normal, field.Status);
        Assert.True(field.CreatedAt != default);
        Assert.True(field.StatusUpdatedAt != default);
    }

    [Fact]
    public void AddField_ShouldRejectDuplicateFieldCode()
    {
        var farm = new Farm("Fazenda A", "Campinas", "SP");
        farm.AddField("T01", "Talhão 01", 10);

        var ex = Assert.Throws<DomainException>(() =>
            farm.AddField("t01", "Talhão 02", 12));

        Assert.Contains("Já existe um talhão com esse código", ex.Message);
    }
}
