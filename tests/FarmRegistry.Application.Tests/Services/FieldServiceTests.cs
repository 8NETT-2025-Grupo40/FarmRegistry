using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
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
    private readonly IValidator<CreateFieldRequest> _createValidator;
    private readonly IValidator<UpdateFieldRequest> _updateValidator;
    private readonly FieldService _fieldService;

    public FieldServiceTests()
    {
        _fieldRepository = Substitute.For<IFieldRepository>();
        _farmRepository = Substitute.For<IFarmRepository>();
        _mapper = Substitute.For<IMapper>();
        _createValidator = Substitute.For<IValidator<CreateFieldRequest>>();
        _updateValidator = Substitute.For<IValidator<UpdateFieldRequest>>();
        
        _fieldService = new FieldService(
            _fieldRepository,
            _farmRepository,
            _mapper,
            _createValidator,
            _updateValidator);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldReturnFieldResponse()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var request = new CreateFieldRequest(farmId, "T01", "Talhão 01", 10.5m, FieldStatus.Normal);
        var field = new Field(farmId, "T01", "Talhão 01", 10.5);
        var response = new FieldResponse(field.FieldId, farmId, "T01", "Talhão 01", 10.5m, FieldStatus.Normal, field.StatusUpdatedAt, field.CreatedAt);

        _createValidator.ValidateAsync(request, default)
            .Returns(new ValidationResult());
        
        _farmRepository.ExistsAsync(farmId, default)
            .Returns(true);
        
        _fieldRepository.CodeExistsInFarmAsync(farmId, "T01", null, default)
            .Returns(false);
        
        _fieldRepository.CreateAsync(Arg.Any<Field>(), default)
            .Returns(field);
        
        _mapper.Map<FieldResponse>(Arg.Any<Field>())
            .Returns(response);

        // Act
        var result = await _fieldService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("T01", result.Code);
        Assert.Equal("Talhão 01", result.Name);
        Assert.Equal(10.5m, result.AreaHectares);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRequest_ShouldThrowDomainException()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var request = new CreateFieldRequest(farmId, "", "Talhão 01", 10.5m, FieldStatus.Normal);
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Code", "Código é obrigatório")
        });

        _createValidator.ValidateAsync(request, default)
            .Returns(validationResult);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.CreateAsync(request));
        
        Assert.Contains("Código é obrigatório", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentFarm_ShouldThrowDomainException()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var request = new CreateFieldRequest(farmId, "T01", "Talhão 01", 10.5m, FieldStatus.Normal);

        _createValidator.ValidateAsync(request, default)
            .Returns(new ValidationResult());
        
        _farmRepository.ExistsAsync(farmId, default)
            .Returns(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.CreateAsync(request));
        
        Assert.Contains("Propriedade não encontrada", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateCode_ShouldThrowDomainException()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var request = new CreateFieldRequest(farmId, "T01", "Talhão 01", 10.5m, FieldStatus.Normal);

        _createValidator.ValidateAsync(request, default)
            .Returns(new ValidationResult());
        
        _farmRepository.ExistsAsync(farmId, default)
            .Returns(true);
        
        _fieldRepository.CodeExistsInFarmAsync(farmId, "T01", null, default)
            .Returns(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.CreateAsync(request));
        
        Assert.Contains("Já existe um talhão com esse código", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldReturnUpdatedFieldResponse()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var request = new UpdateFieldRequest(fieldId, farmId, "T02", "Talhão 02", 12.5m, FieldStatus.AlertaSeca);
        var existingField = new Field(farmId, "T01", "Talhão 01", 10.5);
        var response = new FieldResponse(fieldId, farmId, "T02", "Talhão 02", 12.5m, FieldStatus.AlertaSeca, DateTime.UtcNow, existingField.CreatedAt);

        _updateValidator.ValidateAsync(request, default)
            .Returns(new ValidationResult());
        
        _fieldRepository.GetByIdAsync(fieldId, default)
            .Returns(existingField);
        
        _farmRepository.ExistsAsync(farmId, default)
            .Returns(true);
        
        _fieldRepository.CodeExistsInFarmAsync(farmId, "T02", fieldId, default)
            .Returns(false);
        
        _fieldRepository.UpdateAsync(existingField, default)
            .Returns(existingField);
        
        _mapper.Map<FieldResponse>(existingField)
            .Returns(response);

        // Act
        var result = await _fieldService.UpdateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("T02", result.Code);
        Assert.Equal("Talhão 02", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentField_ShouldThrowDomainException()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var request = new UpdateFieldRequest(fieldId, farmId, "T02", "Talhão 02", 12.5m, FieldStatus.AlertaSeca);

        _updateValidator.ValidateAsync(request, default)
            .Returns(new ValidationResult());
        
        _fieldRepository.GetByIdAsync(fieldId, default)
            .Returns((Field?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.UpdateAsync(request));
        
        Assert.Contains("Talhão não encontrado", ex.Message);
    }

    [Fact]
    public async Task GetByFarmIdAsync_WithValidFarmId_ShouldReturnFields()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var fields = new[]
        {
            new Field(farmId, "T01", "Talhão 01", 10.5),
            new Field(farmId, "T02", "Talhão 02", 8.0)
        };
        var responses = fields.Select(f => new FieldResponse(f.FieldId, f.FarmId, f.Code, f.Name, (decimal)f.AreaHectares, f.Status, f.StatusUpdatedAt, f.CreatedAt));

        _farmRepository.ExistsAsync(farmId, default)
            .Returns(true);
        
        _fieldRepository.GetByFarmIdAsync(farmId, default)
            .Returns(fields);
        
        _mapper.Map<IEnumerable<FieldResponse>>(fields)
            .Returns(responses);

        // Act
        var result = await _fieldService.GetByFarmIdAsync(farmId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByFarmIdAsync_WithNonExistentFarm_ShouldThrowDomainException()
    {
        // Arrange
        var farmId = Guid.NewGuid();

        _farmRepository.ExistsAsync(farmId, default)
            .Returns(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.GetByFarmIdAsync(farmId));
        
        Assert.Contains("Propriedade não encontrada", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingField_ShouldReturnFieldResponse()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 10.5);
        var response = new FieldResponse(fieldId, farmId, "T01", "Talhão 01", 10.5m, FieldStatus.Normal, field.StatusUpdatedAt, field.CreatedAt);

        _fieldRepository.GetByIdAsync(fieldId, default)
            .Returns(field);
        
        _mapper.Map<FieldResponse>(field)
            .Returns(response);

        // Act
        var result = await _fieldService.GetByIdAsync(fieldId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("T01", result.Code);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentField_ShouldReturnNull()
    {
        // Arrange
        var fieldId = Guid.NewGuid();

        _fieldRepository.GetByIdAsync(fieldId, default)
            .Returns((Field?)null);

        // Act
        var result = await _fieldService.GetByIdAsync(fieldId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ActivateAsync_WithExistingField_ShouldActivateField()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 10.5);
        field.Deactivate(); // Primeiro desativa

        _fieldRepository.GetByIdAsync(fieldId, default)
            .Returns(field);

        // Act
        await _fieldService.ActivateAsync(fieldId);

        // Assert
        Assert.Equal(FieldStatus.Normal, field.Status);
        await _fieldRepository.Received(1).UpdateAsync(field, default);
    }

    [Fact]
    public async Task ActivateAsync_WithNonExistentField_ShouldThrowDomainException()
    {
        // Arrange
        var fieldId = Guid.NewGuid();

        _fieldRepository.GetByIdAsync(fieldId, default)
            .Returns((Field?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.ActivateAsync(fieldId));
        
        Assert.Contains("Talhão não encontrado", ex.Message);
    }

    [Fact]
    public async Task DeactivateAsync_WithExistingField_ShouldDeactivateField()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 10.5);

        _fieldRepository.GetByIdAsync(fieldId, default)
            .Returns(field);

        // Act
        await _fieldService.DeactivateAsync(fieldId);

        // Assert
        Assert.Equal(FieldStatus.Inativo, field.Status);
        await _fieldRepository.Received(1).UpdateAsync(field, default);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingField_ShouldDeactivateField()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var field = new Field(farmId, "T01", "Talhão 01", 10.5);

        _fieldRepository.GetByIdAsync(fieldId, default)
            .Returns(field);

        // Act
        await _fieldService.DeleteAsync(fieldId);

        // Assert
        Assert.Equal(FieldStatus.Inativo, field.Status);
        await _fieldRepository.Received(1).UpdateAsync(field, default);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentField_ShouldThrowDomainException()
    {
        // Arrange
        var fieldId = Guid.NewGuid();

        _fieldRepository.GetByIdAsync(fieldId, default)
            .Returns((Field?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _fieldService.DeleteAsync(fieldId));
        
        Assert.Contains("Talhão não encontrado", ex.Message);
    }
}