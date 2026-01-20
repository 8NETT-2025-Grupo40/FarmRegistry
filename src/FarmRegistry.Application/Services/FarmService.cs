using AutoMapper;
using FluentValidation;
using FarmRegistry.Application.Contracts.Common;
using FarmRegistry.Application.Contracts.Farms;
using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Services;

public sealed class FarmService
{
    private readonly IFarmRepository _farmRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateFarmRequest> _createValidator;
    private readonly IValidator<UpdateFarmRequest> _updateValidator;

    public FarmService(
        IFarmRepository farmRepository,
        IUserContext userContext,
        IMapper mapper,
        IValidator<CreateFarmRequest> createValidator,
        IValidator<UpdateFarmRequest> updateValidator)
    {
        _farmRepository = farmRepository;
        _userContext = userContext;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<FarmResponse> CreateAsync(CreateFarmRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var farm = new Farm(request.Name, request.City, request.State);
        
        await _farmRepository.CreateAsync(farm, cancellationToken);
        
        return _mapper.Map<FarmResponse>(farm);
    }

    public async Task<FarmResponse> UpdateAsync(UpdateFarmRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var farm = await _farmRepository.GetByIdAsync(request.Id, cancellationToken);
        if (farm is null)
            throw new DomainException("Propriedade não encontrada.");

        farm.Update(request.Name, request.City, request.State);
        
        await _farmRepository.UpdateAsync(farm, cancellationToken);
        
        return _mapper.Map<FarmResponse>(farm);
    }

    public async Task<IEnumerable<FarmResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var farms = await _farmRepository.GetAllAsync(_userContext.OwnerId, cancellationToken);
        return _mapper.Map<IEnumerable<FarmResponse>>(farms);
    }

    public async Task<FarmResponse?> GetByIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId, cancellationToken);
        return farm is null ? null : _mapper.Map<FarmResponse>(farm);
    }

    public async Task ActivateAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId, cancellationToken);
        if (farm is null)
            throw new DomainException("Propriedade não encontrada.");

        farm.Activate();
        await _farmRepository.UpdateAsync(farm, cancellationToken);
    }

    public async Task DeactivateAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId, cancellationToken);
        if (farm is null)
            throw new DomainException("Propriedade não encontrada.");

        farm.Deactivate();
        await _farmRepository.UpdateAsync(farm, cancellationToken);
    }

    public async Task DeleteAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId, cancellationToken);
        if (farm is null)
            throw new DomainException("Propriedade não encontrada.");

        farm.Deactivate(); // Delete lógico
        await _farmRepository.UpdateAsync(farm, cancellationToken);
    }
}