using AutoMapper;
using FarmRegistry.Application.Contracts.Farms;
using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Application.Services;
using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;
using NSubstitute;

namespace FarmRegistry.Application.Tests.Services;

public sealed class FarmServiceTests
{
    private readonly IFarmRepository _farmRepository;
    private readonly IMapper _mapper;
    private readonly FarmService _farmService;

    public FarmServiceTests()
    {
        _farmRepository = Substitute.For<IFarmRepository>();
        _mapper = Substitute.For<IMapper>();
        
        _farmService = new FarmService(_farmRepository, _mapper);
    }

    [Fact]
    public async Task CreateFarmAsync_WithValidRequest_ShouldReturnFarmResponse()
    {
        // Arrange
        var request = new CreateFarmRequest("Fazenda Teste", "Campinas", "SP");
        var farm = new Farm("Fazenda Teste", "Campinas", "SP");
        var response = new FarmResponse(farm.FarmId, "Fazenda Teste", "Campinas", "SP", true, farm.CreatedAt);

        _farmRepository.CreateAsync(Arg.Any<Farm>(), default)
            .Returns(farm);
        
        _mapper.Map<FarmResponse>(Arg.Any<Farm>())
            .Returns(response);

        // Act
        var result = await _farmService.CreateFarmAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fazenda Teste", result.Name);
        Assert.Equal("Campinas", result.City);
        Assert.Equal("SP", result.State);
    }

    [Fact]
    public async Task UpdateFarmAsync_WithValidRequest_ShouldReturnUpdatedFarmResponse()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var request = new UpdateFarmRequest(farmId, "Fazenda Atualizada", "Sorocaba", "SP");
        var existingFarm = new Farm("Fazenda Original", "Campinas", "SP");
        var response = new FarmResponse(farmId, "Fazenda Atualizada", "Sorocaba", "SP", true, existingFarm.CreatedAt);

        _farmRepository.GetByIdAsync(farmId, default)
            .Returns(existingFarm);
        
        _farmRepository.UpdateAsync(existingFarm, default)
            .Returns(existingFarm);
        
        _mapper.Map<FarmResponse>(existingFarm)
            .Returns(response);

        // Act
        var result = await _farmService.UpdateFarmAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fazenda Atualizada", result.Name);
        Assert.Equal("Sorocaba", result.City);
    }

    [Fact]
    public async Task UpdateFarmAsync_WithNonExistentFarm_ShouldThrowDomainException()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var request = new UpdateFarmRequest(farmId, "Fazenda Teste", "Campinas", "SP");

        _farmRepository.GetByIdAsync(farmId, default)
            .Returns((Farm?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _farmService.UpdateFarmAsync(request));
        
        Assert.Contains("Fazenda com ID", ex.Message);
    }

    [Fact]
    public async Task GetFarmsAsync_ShouldReturnFarmsForOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farms = new[]
        {
            new Farm("Fazenda 1", "Campinas", "SP"),
            new Farm("Fazenda 2", "Sorocaba", "SP")
        };
        var responses = farms.Select(f => new FarmResponse(f.FarmId, f.Name, f.City, f.State, true, f.CreatedAt));

        _farmRepository.GetAllAsync(ownerId, default)
            .Returns(farms);
        
        _mapper.Map<IEnumerable<FarmResponse>>(farms)
            .Returns(responses);

        // Act
        var result = await _farmService.GetFarmsAsync(ownerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ActivateFarmAsync_WithExistingFarm_ShouldActivateFarm()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var farm = new Farm("Fazenda Teste", "Campinas", "SP");
        var response = new FarmResponse(farmId, "Fazenda Teste", "Campinas", "SP", true, farm.CreatedAt);
        
        farm.Deactivate(); // Primeiro desativa

        _farmRepository.GetByIdAsync(farmId, default)
            .Returns(farm);
        
        _farmRepository.UpdateAsync(farm, default)
            .Returns(farm);
        
        _mapper.Map<FarmResponse>(farm)
            .Returns(response);

        // Act
        var result = await _farmService.ActivateFarmAsync(farmId);

        // Assert
        Assert.True(farm.IsActive);
        Assert.NotNull(result);
        await _farmRepository.Received(1).UpdateAsync(farm, default);
    }

    [Fact]
    public async Task DeleteFarmAsync_WithExistingFarm_ShouldCallDeleteAsync()
    {
        // Arrange
        var farmId = Guid.NewGuid();

        _farmRepository.ExistsAsync(farmId, default)
            .Returns(true);

        // Act
        await _farmService.DeleteFarmAsync(farmId);

        // Assert
        await _farmRepository.Received(1).DeleteAsync(farmId, default);
    }
}