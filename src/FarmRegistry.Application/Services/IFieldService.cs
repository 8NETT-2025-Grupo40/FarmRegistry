using FarmRegistry.Application.Contracts.Fields;

namespace FarmRegistry.Application.Services;

public interface IFieldService
{
    Task<FieldResponse> CreateFieldAsync(Guid ownerId, CreateFieldRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<FieldResponse>> GetFieldsByFarmIdAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default);
    Task<FieldResponse?> GetFieldByIdAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default);
    Task<FieldResponse> UpdateFieldAsync(Guid ownerId, UpdateFieldRequest request, CancellationToken cancellationToken = default);
    Task<FieldResponse> ActivateFieldAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default);
    Task<FieldResponse> DeactivateFieldAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default);
    Task DeleteFieldAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default);
}
