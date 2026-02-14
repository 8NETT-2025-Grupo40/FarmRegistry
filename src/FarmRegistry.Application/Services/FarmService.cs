using AutoMapper;
using FarmRegistry.Application.Contracts.Farms;
using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Services;

public sealed class FarmService : IFarmService
{
    private readonly IFarmRepository _farmRepository;
    private readonly IMapper _mapper;

    public FarmService(IFarmRepository farmRepository, IMapper mapper)
    {
        _farmRepository = farmRepository;
        _mapper = mapper;
    }

    public async Task<FarmResponse> CreateFarmAsync(Guid ownerId, CreateFarmRequest request, CancellationToken cancellationToken = default)
    {
        var farm = new Farm(ownerId, request.Name, request.City, request.State);
        var createdFarm = await _farmRepository.CreateAsync(farm, cancellationToken);
        return _mapper.Map<FarmResponse>(createdFarm);
    }

    public async Task<IEnumerable<FarmResponse>> GetFarmsAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var farms = await _farmRepository.GetAllAsync(ownerId, cancellationToken);
        return _mapper.Map<IEnumerable<FarmResponse>>(farms);
    }

    public async Task<FarmResponse?> GetFarmByIdAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(ownerId, farmId, cancellationToken);
        return farm == null ? null : _mapper.Map<FarmResponse>(farm);
    }

    public async Task<FarmResponse> UpdateFarmAsync(Guid ownerId, UpdateFarmRequest request, CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(ownerId, request.Id, cancellationToken);
        if (farm == null)
            throw new DomainException($"Fazenda com ID {request.Id} não foi encontrada.");

        farm.Update(request.Name, request.City, request.State);
        var updatedFarm = await _farmRepository.UpdateAsync(farm, cancellationToken);
        return _mapper.Map<FarmResponse>(updatedFarm);
    }

    public async Task<FarmResponse> ActivateFarmAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(ownerId, farmId, cancellationToken);
        if (farm == null)
            throw new DomainException($"Fazenda com ID {farmId} não foi encontrada.");

        farm.Activate();
        var updatedFarm = await _farmRepository.UpdateAsync(farm, cancellationToken);
        return _mapper.Map<FarmResponse>(updatedFarm);
    }

    public async Task<FarmResponse> DeactivateFarmAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(ownerId, farmId, cancellationToken);
        if (farm == null)
            throw new DomainException($"Fazenda com ID {farmId} não foi encontrada.");

        farm.Deactivate();
        var updatedFarm = await _farmRepository.UpdateAsync(farm, cancellationToken);
        return _mapper.Map<FarmResponse>(updatedFarm);
    }

    public async Task DeleteFarmAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default)
    {
        var farmExists = await _farmRepository.ExistsAsync(ownerId, farmId, cancellationToken);
        if (!farmExists)
            throw new DomainException($"Fazenda com ID {farmId} não foi encontrada.");

        await _farmRepository.DeleteAsync(ownerId, farmId, cancellationToken);
    }
}
