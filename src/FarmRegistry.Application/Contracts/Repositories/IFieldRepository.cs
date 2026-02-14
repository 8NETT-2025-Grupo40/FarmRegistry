using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Contracts.Repositories;

public interface IFieldRepository
{
    Task<Field?> GetByIdAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Field>> GetByFarmIdAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default);
    Task<Field> CreateAsync(Field field, CancellationToken cancellationToken = default);
    Task<Field> UpdateAsync(Field field, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsInFarmAsync(Guid ownerId, Guid farmId, string code, Guid? excludeFieldId = null, CancellationToken cancellationToken = default);
}
