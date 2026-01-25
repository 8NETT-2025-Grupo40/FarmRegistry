using FarmRegistry.Application.Contracts.Fields;

namespace FarmRegistry.Application.Services;

public interface IFieldService
{
    Task<FieldResponse> CreateFieldAsync(CreateFieldRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<FieldResponse>> GetFieldsByFarmIdAsync(Guid farmId, CancellationToken cancellationToken = default);
    Task<FieldResponse?> GetFieldByIdAsync(Guid fieldId, CancellationToken cancellationToken = default);
    Task<FieldResponse> UpdateFieldAsync(UpdateFieldRequest request, CancellationToken cancellationToken = default);
    Task<FieldResponse> ActivateFieldAsync(Guid fieldId, CancellationToken cancellationToken = default);
    Task<FieldResponse> DeactivateFieldAsync(Guid fieldId, CancellationToken cancellationToken = default);
    Task DeleteFieldAsync(Guid fieldId, CancellationToken cancellationToken = default);
}