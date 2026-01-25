using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Api.Infrastructure;

public class MockFarmRepository : IFarmRepository
{
    private readonly List<Farm> _farms = new();

    public Task<Farm?> GetByIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = _farms.FirstOrDefault(f => f.FarmId == farmId);
        return Task.FromResult(farm);
    }

    public Task<IEnumerable<Farm>> GetAllAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Farm>>(_farms);
    }

    public Task<Farm> CreateAsync(Farm farm, CancellationToken cancellationToken = default)
    {
        _farms.Add(farm);
        return Task.FromResult(farm);
    }

    public Task<Farm> UpdateAsync(Farm farm, CancellationToken cancellationToken = default)
    {
        var existingFarmIndex = _farms.FindIndex(f => f.FarmId == farm.FarmId);
        if (existingFarmIndex >= 0)
        {
            _farms[existingFarmIndex] = farm;
        }
        return Task.FromResult(farm);
    }

    public Task DeleteAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = _farms.FirstOrDefault(f => f.FarmId == farmId);
        if (farm != null)
        {
            farm.Deactivate();
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var exists = _farms.Any(f => f.FarmId == farmId);
        return Task.FromResult(exists);
    }
}