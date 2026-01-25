using FarmRegistry.Application.Contracts.Farms;

namespace FarmRegistry.Application.Services;

public interface IFarmService
{
    Task<FarmResponse> CreateFarmAsync(CreateFarmRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<FarmResponse>> GetFarmsAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<FarmResponse?> GetFarmByIdAsync(Guid farmId, CancellationToken cancellationToken = default);
    Task<FarmResponse> UpdateFarmAsync(UpdateFarmRequest request, CancellationToken cancellationToken = default);
    Task<FarmResponse> ActivateFarmAsync(Guid farmId, CancellationToken cancellationToken = default);
    Task<FarmResponse> DeactivateFarmAsync(Guid farmId, CancellationToken cancellationToken = default);
    Task DeleteFarmAsync(Guid farmId, CancellationToken cancellationToken = default);
}