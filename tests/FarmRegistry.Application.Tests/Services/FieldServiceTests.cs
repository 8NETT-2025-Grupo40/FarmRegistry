using AutoMapper;
using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Application.Services;
using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;
using NSubstitute;

namespace FarmRegistry.Application.Tests.Services;

public sealed class FieldServiceTests
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IMapper _mapper;
    private readonly FieldService _fieldService;

    private static IReadOnlyCollection<FieldBoundaryPointRequest> BoundaryRequest =>
    [
        new FieldBoundaryPointRequest(-21.2211, -47.8301),
        new FieldBoundaryPointRequest(-21.2208, -47.8296),
        new FieldBoundaryPointRequest(-21.2215, -47.8291)
    ];

    private static IReadOnlyCollection<FieldBoundaryPointRequest> UpdatedBoundaryRequest =>
    [
        new FieldBoundaryPointRequest(-21.2231, -47.8311),
        new FieldBoundaryPointRequest(-21.2227, -47.8305),
        new FieldBoundaryPointRequest(-21.2234, -47.8302)
    ];

    private static IReadOnlyCollection<FieldBoundaryCoordinate> BoundaryCoordinates =>
    [
        new FieldBoundaryCoordinate(-21.2211, -47.8301),
        new FieldBoundaryCoordinate(-21.2208, -47.8296),
        new FieldBoundaryCoordinate(-21.2215, -47.8291)
    ];

    private static IReadOnlyCollection<FieldBoundaryPointResponse> BoundaryResponse =>
    [
        new FieldBoundaryPointResponse(1, -21.2211, -47.8301),
        new FieldBoundaryPointResponse(2, -21.2208, -47.8296),
        new FieldBoundaryPointResponse(3, -21.2215, -47.8291)
    ];

    public FieldServiceTests()
    {
        _fieldRepository = Substitute.For<IFieldRepository>();
        _farmRepository = Substitute.For<IFarmRepository>();
        _mapper = Substitute.For<IMapper>();
        
        _fieldService = new FieldService(_fieldRepository, _farmRepository, _mapper);
    }

    [Fact]
    public async Task CreateFieldAsync_WithValidRequest_ShouldReturnFieldResponse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var request = new CreateFieldRequest(farmId, "T01", "Talhão 01", 10.5m, "Milho", BoundaryRequest, FieldStatus.Normal);
        var field = new Field(farmId, "T01", "Talhão 01", 10.5, "Milho", BoundaryCoordinates);
        var response = new FieldResponse(field.FieldId, farmId, "T01", "Talhão 01", 10.5m, "Milho", BoundaryResponse, FieldStatus.Normal, field.StatusUpdatedAt, field.CreatedAt);

        _farmRepository.ExistsAsync(ownerId, farmId, default)
            .Returns(true);
        
        _fieldRepository.CodeExistsInFarmAsync(ownerId, farmId, "T01", null, default)
            .Returns(false);
        
        _fieldRepository.CreateAsync(Arg.Any<Field>(), default)
            .Returns(field);
        
        _mapper.Map<FieldResponse>(Arg.Any<Field>())
            .Returns(response);

        // Act
        var result = await _fieldService.CreateFieldAsync(ownerId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("T01", result.Code);
        Assert.Equal("Talhão 01", result.Name);
        Assert.Equal(10.5m, result.AreaHectares);
        Assert.Equal("Milho", result.CropName);
        Assert.Equal(3, result.BoundaryPoints.Count);
    }

    [Fact]
    public async Task CreateFieldAsync_WithNonExistentFarm_ShouldThrowDomainException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var request = new CreateFieldRequest(farmId, "T01", "Talhão 01", 10.5m, "Milho", BoundaryRequest, FieldStatus.Normal);

        _farmRepository.ExistsAsync(ownerId, farmId, default)
            .Returns(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.CreateFieldAsync(ownerId, request));
        
        Assert.Contains("Fazenda com ID", ex.Message);
    }

    [Fact]
    public async Task CreateFieldAsync_WithDuplicateCode_ShouldThrowDomainException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var request = new CreateFieldRequest(farmId, "T01", "Talhão 01", 10.5m, "Milho", BoundaryRequest, FieldStatus.Normal);

        _farmRepository.ExistsAsync(ownerId, farmId, default)
            .Returns(true);
        
        _fieldRepository.CodeExistsInFarmAsync(ownerId, farmId, "T01", null, default)
            .Returns(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.CreateFieldAsync(ownerId, request));
        
        Assert.Contains("Já existe um talhão", ex.Message);
    }

    [Fact]
    public async Task UpdateFieldAsync_WithValidRequest_ShouldReturnUpdatedFieldResponse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var request = new UpdateFieldRequest(fieldId, farmId, "T02", "Talhão 02", 12.5m, "Soja", UpdatedBoundaryRequest, FieldStatus.AlertaSeca);
        var existingField = new Field(farmId, "T01", "Talhão 01", 10.5, "Milho", BoundaryCoordinates);
        var response = new FieldResponse(fieldId, farmId, "T02", "Talhão 02", 12.5m, "Soja", BoundaryResponse, FieldStatus.AlertaSeca, DateTime.UtcNow, existingField.CreatedAt);

        _fieldRepository.GetByIdAsync(ownerId, fieldId, default)
            .Returns(existingField);

        _fieldRepository.CodeExistsInFarmAsync(ownerId, farmId, "T02", fieldId, default)
            .Returns(false);
        
        _fieldRepository.UpdateAsync(existingField, default)
            .Returns(existingField);
        
        _mapper.Map<FieldResponse>(existingField)
            .Returns(response);

        // Act
        var result = await _fieldService.UpdateFieldAsync(ownerId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("T02", result.Code);
        Assert.Equal("Talhão 02", result.Name);
        Assert.Equal("Soja", result.CropName);
    }

    [Fact]
    public async Task UpdateFieldAsync_WithDifferentFarmId_ShouldThrowDomainException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var currentFarmId = Guid.NewGuid();
        var anotherFarmId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var request = new UpdateFieldRequest(fieldId, anotherFarmId, "T02", "Talhão 02", 12.5m, "Soja", UpdatedBoundaryRequest, FieldStatus.Normal);
        var existingField = new Field(currentFarmId, "T01", "Talhão 01", 10.5, "Milho", BoundaryCoordinates);

        _fieldRepository.GetByIdAsync(ownerId, fieldId, default)
            .Returns(existingField);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.UpdateFieldAsync(ownerId, request));

        Assert.Contains("Não é permitido alterar a propriedade", ex.Message);
    }

    [Fact]
    public async Task GetFieldsByFarmIdAsync_WithValidFarmId_ShouldReturnFields()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var fields = new[]
        {
            new Field(farmId, "T01", "Talhão 01", 10.5, "Milho", BoundaryCoordinates),
            new Field(farmId, "T02", "Talhão 02", 8.0, "Soja", BoundaryCoordinates)
        };
        var responses = fields.Select(f => new FieldResponse(
            f.FieldId,
            f.FarmId,
            f.Code,
            f.Name,
            (decimal)f.AreaHectares,
            f.CropName,
            BoundaryResponse,
            f.Status,
            f.StatusUpdatedAt,
            f.CreatedAt));

        _farmRepository.ExistsAsync(ownerId, farmId, default)
            .Returns(true);
        
        _fieldRepository.GetByFarmIdAsync(ownerId, farmId, default)
            .Returns(fields);
        
        _mapper.Map<IEnumerable<FieldResponse>>(fields)
            .Returns(responses);

        // Act
        var result = await _fieldService.GetFieldsByFarmIdAsync(ownerId, farmId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetFieldByIdAsync_WithExistingField_ShouldReturnFieldResponse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 10.5, "Milho", BoundaryCoordinates);
        var response = new FieldResponse(fieldId, farmId, "T01", "Talhão 01", 10.5m, "Milho", BoundaryResponse, FieldStatus.Normal, field.StatusUpdatedAt, field.CreatedAt);

        _fieldRepository.GetByIdAsync(ownerId, fieldId, default)
            .Returns(field);
        
        _mapper.Map<FieldResponse>(field)
            .Returns(response);

        // Act
        var result = await _fieldService.GetFieldByIdAsync(ownerId, fieldId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("T01", result.Code);
        Assert.Equal("Milho", result.CropName);
    }

    [Fact]
    public async Task ActivateFieldAsync_WithExistingField_ShouldActivateField()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 10.5, "Milho", BoundaryCoordinates);
        var response = new FieldResponse(fieldId, farmId, "T01", "Talhão 01", 10.5m, "Milho", BoundaryResponse, FieldStatus.Normal, field.StatusUpdatedAt, field.CreatedAt);
        
        field.Deactivate(); // Primeiro desativa

        _fieldRepository.GetByIdAsync(ownerId, fieldId, default)
            .Returns(field);
        
        _fieldRepository.UpdateAsync(field, default)
            .Returns(field);
        
        _mapper.Map<FieldResponse>(field)
            .Returns(response);

        // Act
        var result = await _fieldService.ActivateFieldAsync(ownerId, fieldId);

        // Assert
        Assert.Equal(FieldStatus.Normal, field.Status);
        Assert.NotNull(result);
        await _fieldRepository.Received(1).UpdateAsync(field, default);
    }

    [Fact]
    public async Task DeactivateFieldAsync_WithExistingField_ShouldDeactivateField()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 10.5, "Milho", BoundaryCoordinates);
        var response = new FieldResponse(fieldId, farmId, "T01", "Talhão 01", 10.5m, "Milho", BoundaryResponse, FieldStatus.Inativo, field.StatusUpdatedAt, field.CreatedAt);

        _fieldRepository.GetByIdAsync(ownerId, fieldId, default)
            .Returns(field);
        
        _fieldRepository.UpdateAsync(field, default)
            .Returns(field);
        
        _mapper.Map<FieldResponse>(field)
            .Returns(response);

        // Act
        var result = await _fieldService.DeactivateFieldAsync(ownerId, fieldId);

        // Assert
        Assert.Equal(FieldStatus.Inativo, field.Status);
        Assert.NotNull(result);
        await _fieldRepository.Received(1).UpdateAsync(field, default);
    }

    [Fact]
    public async Task DeleteFieldAsync_WithExistingField_ShouldCallDeleteAsync()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        _fieldRepository.ExistsAsync(ownerId, fieldId, default)
            .Returns(true);

        // Act
        await _fieldService.DeleteFieldAsync(ownerId, fieldId);

        // Assert
        await _fieldRepository.Received(1).DeleteAsync(ownerId, fieldId, default);
    }
}
