using AutoMapper;
using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Services;

public sealed class FieldService : IFieldService
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IMapper _mapper;

    public FieldService(IFieldRepository fieldRepository, IFarmRepository farmRepository, IMapper mapper)
    {
        _fieldRepository = fieldRepository;
        _farmRepository = farmRepository;
        _mapper = mapper;
    }

    public async Task<FieldResponse> CreateFieldAsync(Guid ownerId, CreateFieldRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureFarmAllowsFieldMutationsAsync(ownerId, request.FarmId, cancellationToken);

        // Verificar se já existe um talhão com o mesmo código na fazenda
        var codeExists = await _fieldRepository.CodeExistsInFarmAsync(ownerId, request.FarmId, request.Code, null, cancellationToken);
        if (codeExists)
            throw new DomainException($"Já existe um talhão com o código '{request.Code}' nesta fazenda.");

        var boundaryCoordinates = MapBoundaryCoordinates(request.BoundaryPoints);
        var field = new Field(
            request.FarmId,
            request.Code,
            request.Name,
            (double)request.AreaHectares,
            request.CropName,
            boundaryCoordinates);

        if (request.Status != FieldStatus.Normal)
        {
            field.SetStatus(request.Status);
        }

        var createdField = await _fieldRepository.CreateAsync(field, cancellationToken);
        return _mapper.Map<FieldResponse>(createdField);
    }

    public async Task<IEnumerable<FieldResponse>> GetFieldsByFarmIdAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default)
    {
        var farmExists = await _farmRepository.ExistsAsync(ownerId, farmId, cancellationToken);
        if (!farmExists)
            throw new NotFoundException($"Fazenda com ID {farmId} não foi encontrada.");

        var fields = await _fieldRepository.GetByFarmIdAsync(ownerId, farmId, cancellationToken);
        return _mapper.Map<IEnumerable<FieldResponse>>(fields);
    }

    public async Task<FieldResponse?> GetFieldByIdAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await _fieldRepository.GetByIdAsync(ownerId, fieldId, cancellationToken);
        return field == null ? null : _mapper.Map<FieldResponse>(field);
    }

    public async Task<FieldResponse> UpdateFieldAsync(Guid ownerId, UpdateFieldRequest request, CancellationToken cancellationToken = default)
    {
        var field = await _fieldRepository.GetByIdAsync(ownerId, request.Id, cancellationToken);
        if (field == null)
            throw new NotFoundException($"Talhão com ID {request.Id} não foi encontrado.");

        if (field.FarmId != request.FarmId)
            throw new DomainException("Não é permitido alterar a propriedade vinculada ao talhão.");

        await EnsureFarmAllowsFieldMutationsAsync(ownerId, field.FarmId, cancellationToken);

        // Verificar se já existe outro talhão com o mesmo código na fazenda
        var codeExists = await _fieldRepository.CodeExistsInFarmAsync(ownerId, request.FarmId, request.Code, request.Id, cancellationToken);
        if (codeExists)
            throw new DomainException($"Já existe outro talhão com o código '{request.Code}' nesta fazenda.");

        var boundaryCoordinates = MapBoundaryCoordinates(request.BoundaryPoints);
        field.Update(
            request.Code,
            request.Name,
            (double)request.AreaHectares,
            request.CropName,
            boundaryCoordinates);

        field.SetStatus(request.Status);

        var updatedField = await _fieldRepository.UpdateAsync(field, cancellationToken);
        return _mapper.Map<FieldResponse>(updatedField);
    }

    public async Task<FieldResponse> ActivateFieldAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await _fieldRepository.GetByIdAsync(ownerId, fieldId, cancellationToken);
        if (field == null)
            throw new NotFoundException($"Talhão com ID {fieldId} não foi encontrado.");

        await EnsureFarmAllowsFieldMutationsAsync(ownerId, field.FarmId, cancellationToken);

        field.Activate();
        var updatedField = await _fieldRepository.UpdateAsync(field, cancellationToken);
        return _mapper.Map<FieldResponse>(updatedField);
    }

    public async Task<FieldResponse> DeactivateFieldAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await _fieldRepository.GetByIdAsync(ownerId, fieldId, cancellationToken);
        if (field == null)
            throw new NotFoundException($"Talhão com ID {fieldId} não foi encontrado.");

        await EnsureFarmAllowsFieldMutationsAsync(ownerId, field.FarmId, cancellationToken);

        field.Deactivate();
        var updatedField = await _fieldRepository.UpdateAsync(field, cancellationToken);
        return _mapper.Map<FieldResponse>(updatedField);
    }

    public async Task DeleteFieldAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await _fieldRepository.GetByIdAsync(ownerId, fieldId, cancellationToken);
        if (field == null)
            throw new NotFoundException($"Talhão com ID {fieldId} não foi encontrado.");

        await EnsureFarmAllowsFieldMutationsAsync(ownerId, field.FarmId, cancellationToken);

        await _fieldRepository.DeleteAsync(ownerId, fieldId, cancellationToken);
    }

    private async Task EnsureFarmAllowsFieldMutationsAsync(
        Guid ownerId,
        Guid farmId,
        CancellationToken cancellationToken)
    {
        var farm = await _farmRepository.GetByIdAsync(ownerId, farmId, cancellationToken);
        if (farm == null)
            throw new NotFoundException($"Fazenda com ID {farmId} não foi encontrada.");

        if (!farm.IsActive)
            throw new ConflictException("A fazenda está inativa e não permite alterações em talhões.");
    }

    private static IReadOnlyCollection<FieldBoundaryCoordinate> MapBoundaryCoordinates(
        IReadOnlyCollection<FieldBoundaryPointRequest>? boundaryPoints)
    {
        if (boundaryPoints is null)
            throw new DomainException("A delimitação do talhão é obrigatória.");

        return boundaryPoints
            .Select(point => new FieldBoundaryCoordinate(point.Latitude, point.Longitude))
            .ToList();
    }
}
