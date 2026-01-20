using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Infrastructure.Repositories;

public sealed class FarmRepository : IFarmRepository
{
    // TODO: Implementar com Entity Framework quando a camada de persistência for criada
    private static readonly Dictionary<Guid, Farm> _farms = new();

    public Task<Farm?> GetByIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        _farms.TryGetValue(farmId, out var farm);
        return Task.FromResult(farm);
    }

    public Task<IEnumerable<Farm>> GetAllAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        // TODO: Filtrar por ownerId quando a entidade Farm incluir essa propriedade
        var farms = _farms.Values.Where(f => f.IsActive).AsEnumerable();
        return Task.FromResult(farms);
    }

    public Task<Farm> CreateAsync(Farm farm, CancellationToken cancellationToken = default)
    {
        _farms[farm.FarmId] = farm;
        return Task.FromResult(farm);
    }

    public Task<Farm> UpdateAsync(Farm farm, CancellationToken cancellationToken = default)
    {
        _farms[farm.FarmId] = farm;
        return Task.FromResult(farm);
    }

    public Task DeleteAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        _farms.Remove(farmId);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_farms.ContainsKey(farmId));
    }
}