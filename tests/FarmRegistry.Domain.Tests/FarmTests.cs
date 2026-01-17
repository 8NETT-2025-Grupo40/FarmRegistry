using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Domain.Tests;

public class FarmTests
{
    [Fact]
    public void Create_Farm_ShouldStartActive_AndHaveValidData()
    {
        var farm = new Farm("Fazenda Boa Vista", "João");

        Assert.NotEqual(Guid.Empty, farm.Id);
        Assert.True(farm.IsActive);
        Assert.Equal("Fazenda Boa Vista", farm.Name);
        Assert.Equal("João", farm.OwnerName);
        Assert.Empty(farm.Fields);
    }

    [Fact]
    public void Update_Farm_ShouldChangeNameAndOwner()
    {
        var farm = new Farm("Fazenda A", "Maria");

        farm.Update("Fazenda B", "Carlos");

        Assert.Equal("Fazenda B", farm.Name);
        Assert.Equal("Carlos", farm.OwnerName);
    }

    [Fact]
    public void ActivateDeactivate_Farm_ShouldToggleIsActive()
    {
        var farm = new Farm("Fazenda A", "Maria");

        farm.Deactivate();
        Assert.False(farm.IsActive);

        farm.Activate();
        Assert.True(farm.IsActive);
    }

    [Fact]
    public void AddField_ShouldCreateFieldLinkedToFarm_AndBeReadOnlyExternally()
    {
        var farm = new Farm("Fazenda A", "Maria");

        var field = farm.AddField("Talhão 01", "Milho", 10);

        Assert.Single(farm.Fields);
        Assert.Equal(farm.Id, field.FarmId);
        Assert.True(field.IsActive);
        Assert.Equal("Talhão 01", field.Name);
        Assert.Equal("Milho", field.Culture);
        Assert.Equal(10, field.AreaHectares);
    }

    [Fact]
    public void AddField_ShouldRejectDuplicateFieldName()
    {
        var farm = new Farm("Fazenda A", "Maria");
        farm.AddField("Talhão 01", "Milho", 10);

        var ex = Assert.Throws<DomainException>(() =>
            farm.AddField("talhão 01", "Soja", 12)); // case-insensitive

        Assert.Contains("Já existe um talhão", ex.Message);
    }
}
