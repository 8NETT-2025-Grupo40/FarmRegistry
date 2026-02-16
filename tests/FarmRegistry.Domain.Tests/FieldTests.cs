using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Domain.Tests;

public class FieldTests
{
    private static readonly IReadOnlyCollection<FieldBoundaryCoordinate> ValidBoundary =
    [
        new FieldBoundaryCoordinate(-21.2221, -47.8231),
        new FieldBoundaryCoordinate(-21.2218, -47.8226),
        new FieldBoundaryCoordinate(-21.2226, -47.8222)
    ];

    [Fact]
    public void Create_Field_ShouldStartWithNormalStatus_AndTimestamps()
    {
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 5.5, "Milho", ValidBoundary);

        Assert.NotEqual(Guid.Empty, field.FieldId);
        Assert.Equal(farmId, field.FarmId);
        Assert.Equal("T01", field.Code);
        Assert.Equal("Talhão 01", field.Name);
        Assert.Equal(5.5, field.AreaHectares);
        Assert.Equal("Milho", field.CropName);
        Assert.Equal(3, field.BoundaryPoints.Count);
        Assert.Equal(1, field.BoundaryPoints.Min(point => point.Sequence));
        Assert.Equal(3, field.BoundaryPoints.Max(point => point.Sequence));

        Assert.Equal(FieldStatus.Normal, field.Status);
        Assert.True(field.CreatedAt != default);
        Assert.True(field.StatusUpdatedAt != default);
    }

    [Fact]
    public void Update_Field_ShouldChangeMainProperties()
    {
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 5.5, "Milho", ValidBoundary);

        IReadOnlyCollection<FieldBoundaryCoordinate> updatedBoundary =
        [
            new FieldBoundaryCoordinate(-21.2201, -47.8211),
            new FieldBoundaryCoordinate(-21.2199, -47.8207),
            new FieldBoundaryCoordinate(-21.2205, -47.8204)
        ];

        field.Update("T02", "Talhão 02", 7, "Soja", updatedBoundary);

        Assert.Equal("T02", field.Code);
        Assert.Equal("Talhão 02", field.Name);
        Assert.Equal(7, field.AreaHectares);
        Assert.Equal("Soja", field.CropName);
        Assert.Equal(3, field.BoundaryPoints.Count);
        Assert.All(field.BoundaryPoints, point => Assert.Equal(field.FieldId, point.FieldId));
    }

    [Fact]
    public void ActivateDeactivate_Field_ShouldChangeStatus()
    {
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 5.5, "Milho", ValidBoundary);

        field.Deactivate();
        Assert.Equal(FieldStatus.Inativo, field.Status);

        field.Activate();
        Assert.Equal(FieldStatus.Normal, field.Status);
    }

    [Fact]
    public void Create_Field_ShouldValidateInputs()
    {
        var farmId = Guid.NewGuid();

        Assert.Throws<DomainException>(() => new Field(Guid.Empty, "T01", "Talhão 01", 1, "Milho", ValidBoundary));
        Assert.Throws<DomainException>(() => new Field(farmId, "", "Talhão 01", 1, "Milho", ValidBoundary));
        Assert.Throws<DomainException>(() => new Field(farmId, "T", "Talhão 01", 1, "Milho", ValidBoundary));
        Assert.Throws<DomainException>(() => new Field(farmId, "T01", "", 1, "Milho", ValidBoundary));
        Assert.Throws<DomainException>(() => new Field(farmId, "T01", "T", 1, "Milho", ValidBoundary));
        Assert.Throws<DomainException>(() => new Field(farmId, "T01", "Talhão 01", 0, "Milho", ValidBoundary));
        Assert.Throws<DomainException>(() => new Field(farmId, "T01", "Talhão 01", 1, "", ValidBoundary));
        Assert.Throws<DomainException>(() => new Field(farmId, "T01", "Talhão 01", 1, "Milho", []));
        Assert.Throws<DomainException>(() => new Field(
            farmId,
            "T01",
            "Talhão 01",
            1,
            "Milho",
            [
                new FieldBoundaryCoordinate(-100, -47),
                new FieldBoundaryCoordinate(-21, -47),
                new FieldBoundaryCoordinate(-21, -46)
            ]));
    }
}
