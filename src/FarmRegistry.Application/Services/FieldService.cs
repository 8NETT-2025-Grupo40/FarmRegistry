using AutoMapper;
using FluentValidation;
using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Common;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Services;

public sealed class FieldService
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateFieldRequest> _createValidator;
    private readonly IValidator<UpdateFieldRequest> _updateValidator;

    public FieldService(
        IFieldRepository fieldRepository,
        IFarmRepository farmRepository,
        IMapper mapper,
        IValidator<CreateFieldRequest> createValidator,
        IValidator<UpdateFieldRequest> updateValidator)
    {
        _fieldRepository = fieldRepository;
        _farmRepository = farmRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<FieldResponse> CreateAsync(CreateFieldRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var farmExists = await _farmRepository.ExistsAsync(request.FarmId, cancellationToken);
        if (!farmExists)
            throw new DomainException("Propriedade não encontrada.");

        var codeExists = await _fieldRepository.CodeExistsInFarmAsync(request.FarmId, request.Code, null, cancellationToken);
        if (codeExists)
            throw new DomainException("Já existe um talhão com esse código nesta propriedade.");

        var field = new Field(request.FarmId, request.Code, request.Name, (double)request.AreaHectares);
        
        if (request.Status != FieldStatus.Normal)
            field.SetStatus(request.Status);
        
        await _fieldRepository.CreateAsync(field, cancellationToken);
        
        return _mapper.Map<FieldResponse>(field);
    }

    public async Task<FieldResponse> UpdateAsync(UpdateFieldRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var field = await _fieldRepository.GetByIdAsync(request.Id, cancellationToken);
        if (field is null)
            throw new DomainException("Talhão não encontrado.");

        var farmExists = await _farmRepository.ExistsAsync(request.FarmId, cancellationToken);
        if (!farmExists)
            throw new DomainException("Propriedade não encontrada.");

        var codeExists = await _fieldRepository.CodeExistsInFarmAsync(request.FarmId, request.Code, request.Id, cancellationToken);
        if (codeExists)
            throw new DomainException("Já existe um talhão com esse código nesta propriedade.");

        field.Update(request.Code, request.Name, (double)request.AreaHectares);
        field.SetStatus(request.Status);
        
        await _fieldRepository.UpdateAsync(field, cancellationToken);
        
        return _mapper.Map<FieldResponse>(field);
    }

    public async Task<IEnumerable<FieldResponse>> GetByFarmIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var farmExists = await _farmRepository.ExistsAsync(farmId, cancellationToken);
        if (!farmExists)
            throw new DomainException("Propriedade não encontrada.");

        var fields = await _fieldRepository.GetByFarmIdAsync(farmId, cancellationToken);
        return _mapper.Map<IEnumerable<FieldResponse>>(fields);
    }

    public async Task<FieldResponse?> GetByIdAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, cancellationToken);
        return field is null ? null : _mapper.Map<FieldResponse>(field);
    }

    public async Task ActivateAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, cancellationToken);
        if (field is null)
            throw new DomainException("Talhão não encontrado.");

        field.Activate();
        await _fieldRepository.UpdateAsync(field, cancellationToken);
    }

    public async Task DeactivateAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, cancellationToken);
        if (field is null)
            throw new DomainException("Talhão não encontrado.");

        field.Deactivate();
        await _fieldRepository.UpdateAsync(field, cancellationToken);
    }

    public async Task DeleteAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, cancellationToken);
        if (field is null)
            throw new DomainException("Talhão não encontrado.");

        field.Deactivate(); // Delete lógico
        await _fieldRepository.UpdateAsync(field, cancellationToken);
    }
}