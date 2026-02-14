using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Contracts.Repositories;

public interface IFarmRepository
{
    Task<Farm?> GetByIdAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Farm>> GetAllAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Farm> CreateAsync(Farm farm, CancellationToken cancellationToken = default);
    Task<Farm> UpdateAsync(Farm farm, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default);
}
