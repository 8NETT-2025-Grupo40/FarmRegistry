using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using FarmRegistry.Application.Contracts.Common;
using FarmRegistry.Application.Contracts.Farms;
using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Application.Services;
using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;
using NSubstitute;
using Xunit;

namespace FarmRegistry.Application.Tests.Services;

public sealed class FarmServiceTests
{
    private readonly IFarmRepository _farmRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateFarmRequest> _createValidator;
    private readonly IValidator<UpdateFarmRequest> _updateValidator;
    private readonly FarmService _farmService;

    public FarmServiceTests()
    {
        _farmRepository = Substitute.For<IFarmRepository>();
        _userContext = Substitute.For<IUserContext>();
        _mapper = Substitute.For<IMapper>();
        _createValidator = Substitute.For<IValidator<CreateFarmRequest>>();
        _updateValidator = Substitute.For<IValidator<UpdateFarmRequest>>();
        
        _farmService = new FarmService(
            _farmRepository,
            _userContext,
            _mapper,
            _createValidator,
            _updateValidator);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldReturnFarmResponse()
    {
        // Arrange
        var request = new CreateFarmRequest("Fazenda Teste", "Campinas", "SP");
        var farm = new Farm("Fazenda Teste", "Campinas", "SP");
        var response = new FarmResponse(farm.FarmId, "Fazenda Teste", "Campinas", "SP", farm.CreatedAt);

        _createValidator.ValidateAsync(request, default)
            .Returns(new ValidationResult());
        
        _farmRepository.CreateAsync(Arg.Any<Farm>(), default)
            .Returns(farm);
        
        _mapper.Map<FarmResponse>(Arg.Any<Farm>())
            .Returns(response);

        // Act
        var result = await _farmService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fazenda Teste", result.Name);
        Assert.Equal("Campinas", result.City);
        Assert.Equal("SP", result.State);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRequest_ShouldThrowDomainException()
    {
        // Arrange
        var request = new CreateFarmRequest("", "Campinas", "SP");
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "Nome é obrigatório")
        });

        _createValidator.ValidateAsync(request, default)
            .Returns(validationResult);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _farmService.CreateAsync(request));
        
        Assert.Contains("Nome é obrigatório", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldReturnUpdatedFarmResponse()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var request = new UpdateFarmRequest(farmId, "Fazenda Atualizada", "Sorocaba", "SP");
        var existingFarm = new Farm("Fazenda Original", "Campinas", "SP");
        var response = new FarmResponse(farmId, "Fazenda Atualizada", "Sorocaba", "SP", existingFarm.CreatedAt);

        _updateValidator.ValidateAsync(request, default)
            .Returns(new ValidationResult());
        
        _farmRepository.GetByIdAsync(farmId, default)
            .Returns(existingFarm);
        
        _farmRepository.UpdateAsync(existingFarm, default)
            .Returns(existingFarm);
        
        _mapper.Map<FarmResponse>(existingFarm)
            .Returns(response);

        // Act
        var result = await _farmService.UpdateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fazenda Atualizada", result.Name);
        Assert.Equal("Sorocaba", result.City);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentFarm_ShouldThrowDomainException()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var request = new UpdateFarmRequest(farmId, "Fazenda Teste", "Campinas", "SP");

        _updateValidator.ValidateAsync(request, default)
            .Returns(new ValidationResult());
        
        _farmRepository.GetByIdAsync(farmId, default)
            .Returns((Farm?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _farmService.UpdateAsync(request));
        
        Assert.Contains("Propriedade não encontrada", ex.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFarmsForOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farms = new[]
        {
            new Farm("Fazenda 1", "Campinas", "SP"),
            new Farm("Fazenda 2", "Sorocaba", "SP")
        };
        var responses = farms.Select(f => new FarmResponse(f.FarmId, f.Name, f.City, f.State, f.CreatedAt));

        _userContext.OwnerId.Returns(ownerId);
        
        _farmRepository.GetAllAsync(ownerId, default)
            .Returns(farms);
        
        _mapper.Map<IEnumerable<FarmResponse>>(farms)
            .Returns(responses);

        // Act
        var result = await _farmService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ActivateAsync_WithExistingFarm_ShouldActivateFarm()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var farm = new Farm("Fazenda Teste", "Campinas", "SP");
        farm.Deactivate(); // Primeiro desativa

        _farmRepository.GetByIdAsync(farmId, default)
            .Returns(farm);

        // Act
        await _farmService.ActivateAsync(farmId);

        // Assert
        Assert.True(farm.IsActive);
        await _farmRepository.Received(1).UpdateAsync(farm, default);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingFarm_ShouldDeactivateFarm()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var farm = new Farm("Fazenda Teste", "Campinas", "SP");

        _farmRepository.GetByIdAsync(farmId, default)
            .Returns(farm);

        // Act
        await _farmService.DeleteAsync(farmId);

        // Assert
        Assert.False(farm.IsActive);
        await _farmRepository.Received(1).UpdateAsync(farm, default);
    }
}