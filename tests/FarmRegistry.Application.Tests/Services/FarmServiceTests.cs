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
        var ownerId = Guid.NewGuid();
        var request = new CreateFarmRequest("Fazenda Teste", "Campinas", "SP");
        var farm = new Farm(ownerId, "Fazenda Teste", "Campinas", "SP");
        var response = new FarmResponse(farm.FarmId, "Fazenda Teste", "Campinas", "SP", true, farm.CreatedAt);

        _farmRepository.CreateAsync(Arg.Any<Farm>(), default)
            .Returns(farm);
        
        _mapper.Map<FarmResponse>(Arg.Any<Farm>())
            .Returns(response);

        // Act
        var result = await _farmService.CreateFarmAsync(ownerId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fazenda Teste", result.Name);
        Assert.Equal("Campinas", result.City);
        Assert.Equal("SP", result.State);
        await _farmRepository.Received(1).CreateAsync(
            Arg.Is<Farm>(f => f.OwnerId == ownerId),
            default);
    }

    [Fact]
    public async Task UpdateFarmAsync_WithValidRequest_ShouldReturnUpdatedFarmResponse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var request = new UpdateFarmRequest(farmId, "Fazenda Atualizada", "Sorocaba", "SP");
        var existingFarm = new Farm(ownerId, "Fazenda Original", "Campinas", "SP");
        var response = new FarmResponse(farmId, "Fazenda Atualizada", "Sorocaba", "SP", true, existingFarm.CreatedAt);

        _farmRepository.GetByIdAsync(ownerId, farmId, default)
            .Returns(existingFarm);
        
        _farmRepository.UpdateAsync(existingFarm, default)
            .Returns(existingFarm);
        
        _mapper.Map<FarmResponse>(existingFarm)
            .Returns(response);

        // Act
        var result = await _farmService.UpdateFarmAsync(ownerId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fazenda Atualizada", result.Name);
        Assert.Equal("Sorocaba", result.City);
    }

    [Fact]
    public async Task UpdateFarmAsync_WithNonExistentFarm_ShouldThrowNotFoundException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var request = new UpdateFarmRequest(farmId, "Fazenda Teste", "Campinas", "SP");

        _farmRepository.GetByIdAsync(ownerId, farmId, default)
            .Returns((Farm?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _farmService.UpdateFarmAsync(ownerId, request));
        
        Assert.Contains("Fazenda com ID", ex.Message);
    }

    [Fact]
    public async Task GetFarmsAsync_ShouldReturnFarmsForOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farms = new[]
        {
            new Farm(ownerId, "Fazenda 1", "Campinas", "SP"),
            new Farm(ownerId, "Fazenda 2", "Sorocaba", "SP")
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
        var ownerId = Guid.NewGuid();
        var farmId = Guid.NewGuid();
        var farm = new Farm(ownerId, "Fazenda Teste", "Campinas", "SP");
        var response = new FarmResponse(farmId, "Fazenda Teste", "Campinas", "SP", true, farm.CreatedAt);
        
        farm.Deactivate(); // Primeiro desativa

        _farmRepository.GetByIdAsync(ownerId, farmId, default)
            .Returns(farm);
        
        _farmRepository.UpdateAsync(farm, default)
            .Returns(farm);
        
        _mapper.Map<FarmResponse>(farm)
            .Returns(response);

        // Act
        var result = await _farmService.ActivateFarmAsync(ownerId, farmId);

        // Assert
        Assert.True(farm.IsActive);
        Assert.NotNull(result);
        await _farmRepository.Received(1).UpdateAsync(farm, default);
    }

    [Fact]
    public async Task DeleteFarmAsync_WithExistingFarm_ShouldCallDeleteAsync()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var farmId = Guid.NewGuid();

        _farmRepository.ExistsAsync(ownerId, farmId, default)
            .Returns(true);

        // Act
        await _farmService.DeleteFarmAsync(ownerId, farmId);

        // Assert
        await _farmRepository.Received(1).DeleteAsync(ownerId, farmId, default);
    }
}
