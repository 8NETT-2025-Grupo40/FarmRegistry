using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Infrastructure.Repositories;

public sealed class FieldRepository : IFieldRepository
{
    // TODO: Implementar com Entity Framework quando a camada de persistência for criada
    private static readonly Dictionary<Guid, Field> _fields = new();

    public Task<Field?> GetByIdAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        _fields.TryGetValue(fieldId, out var field);
        return Task.FromResult(field);
    }

    public Task<IEnumerable<Field>> GetByFarmIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var fields = _fields.Values.Where(f => f.FarmId == farmId).AsEnumerable();
        return Task.FromResult(fields);
    }

    public Task<Field> CreateAsync(Field field, CancellationToken cancellationToken = default)
    {
        _fields[field.FieldId] = field;
        return Task.FromResult(field);
    }

    public Task<Field> UpdateAsync(Field field, CancellationToken cancellationToken = default)
    {
        _fields[field.FieldId] = field;
        return Task.FromResult(field);
    }

    public Task DeleteAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        _fields.Remove(fieldId);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_fields.ContainsKey(fieldId));
    }

    public Task<bool> CodeExistsInFarmAsync(Guid farmId, string code, Guid? excludeFieldId = null, CancellationToken cancellationToken = default)
    {
        var exists = _fields.Values.Any(f => 
            f.FarmId == farmId && 
            string.Equals(f.Code, code, StringComparison.OrdinalIgnoreCase) &&
            (excludeFieldId == null || f.FieldId != excludeFieldId));
            
        return Task.FromResult(exists);
    }
}