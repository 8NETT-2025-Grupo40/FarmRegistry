using FarmRegistry.Application.Contracts.Farms;

namespace FarmRegistry.Application.Services;

public interface IFarmService
{
    Task<FarmResponse> CreateFarmAsync(Guid ownerId, CreateFarmRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<FarmResponse>> GetFarmsAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<FarmResponse?> GetFarmByIdAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default);
    Task<FarmResponse> UpdateFarmAsync(Guid ownerId, UpdateFarmRequest request, CancellationToken cancellationToken = default);
    Task<FarmResponse> ActivateFarmAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default);
    Task<FarmResponse> DeactivateFarmAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default);
    Task DeleteFarmAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default);
}
