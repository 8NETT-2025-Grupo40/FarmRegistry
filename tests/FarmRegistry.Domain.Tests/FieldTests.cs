using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Domain.Tests;

public class FieldTests
{
    [Fact]
    public void Create_Field_ShouldStartWithNormalStatus_AndTimestamps()
    {
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 5.5);

        Assert.NotEqual(Guid.Empty, field.FieldId);
        Assert.Equal(farmId, field.FarmId);
        Assert.Equal("T01", field.Code);
        Assert.Equal("Talhão 01", field.Name);
        Assert.Equal(5.5, field.AreaHectares);

        Assert.Equal(FieldStatus.Normal, field.Status);
        Assert.True(field.CreatedAt != default);
        Assert.True(field.StatusUpdatedAt != default);
    }

    [Fact]
    public void Update_Field_ShouldChangeMainProperties()
    {
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 5.5);

        field.Update("T02", "Talhão 02", 7);

        Assert.Equal("T02", field.Code);
        Assert.Equal("Talhão 02", field.Name);
        Assert.Equal(7, field.AreaHectares);
    }

    [Fact]
    public void ActivateDeactivate_Field_ShouldChangeStatus()
    {
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 5.5);

        field.Deactivate();
        Assert.Equal(FieldStatus.Inativo, field.Status);

        field.Activate();
        Assert.Equal(FieldStatus.Normal, field.Status);
    }

    [Fact]
    public void Create_Field_ShouldValidateInputs()
    {
        var farmId = Guid.NewGuid();

        Assert.Throws<DomainException>(() => new Field(Guid.Empty, "T01", "Talhão 01", 1));
        Assert.Throws<DomainException>(() => new Field(farmId, "", "Talhão 01", 1));
        Assert.Throws<DomainException>(() => new Field(farmId, "T", "Talhão 01", 1));
        Assert.Throws<DomainException>(() => new Field(farmId, "T01", "", 1));
        Assert.Throws<DomainException>(() => new Field(farmId, "T01", "T", 1));
        Assert.Throws<DomainException>(() => new Field(farmId, "T01", "Talhão 01", 0));
    }
}
