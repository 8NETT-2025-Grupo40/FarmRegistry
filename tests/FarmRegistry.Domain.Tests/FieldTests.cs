using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Domain.Tests;

public class FieldTests
{
    [Fact]
    public void Create_Field_ShouldStartActive_AndHaveValidData()
    {
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "Talhão 01", "Soja", 5.5);

        Assert.NotEqual(Guid.Empty, field.Id);
        Assert.Equal(farmId, field.FarmId);
        Assert.True(field.IsActive);
        Assert.Equal("Talhão 01", field.Name);
        Assert.Equal("Soja", field.Culture);
        Assert.Equal(5.5, field.AreaHectares);
    }

    [Fact]
    public void Update_Field_ShouldChangeProperties()
    {
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "Talhão 01", "Soja", 5.5);

        field.Update("Talhão 02", "Milho", 7);

        Assert.Equal("Talhão 02", field.Name);
        Assert.Equal("Milho", field.Culture);
        Assert.Equal(7, field.AreaHectares);
    }

    [Fact]
    public void ActivateDeactivate_Field_ShouldToggleIsActive()
    {
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "Talhão 01", "Soja", 5.5);

        field.Deactivate();
        Assert.False(field.IsActive);

        field.Activate();
        Assert.True(field.IsActive);
    }

    [Fact]
    public void Create_Field_ShouldValidateInputs()
    {
        var farmId = Guid.NewGuid();

        Assert.Throws<DomainException>(() => new Field(Guid.Empty, "Talhão 01", "Soja", 1));
        Assert.Throws<DomainException>(() => new Field(farmId, "", "Soja", 1));
        Assert.Throws<DomainException>(() => new Field(farmId, "T", "Soja", 1));
        Assert.Throws<DomainException>(() => new Field(farmId, "Talhão 01", "", 1));
        Assert.Throws<DomainException>(() => new Field(farmId, "Talhão 01", "Soja", 0));
    }
}
