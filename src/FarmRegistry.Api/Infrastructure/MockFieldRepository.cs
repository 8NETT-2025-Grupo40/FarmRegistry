using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Api.Infrastructure;

public class MockFieldRepository : IFieldRepository
{
    private readonly List<Field> _fields = new();

    public Task<Field?> GetByIdAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = _fields.FirstOrDefault(f => f.FieldId == fieldId);
        return Task.FromResult(field);
    }

    public Task<IEnumerable<Field>> GetByFarmIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var fields = _fields.Where(f => f.FarmId == farmId);
        return Task.FromResult(fields);
    }

    public Task<Field> CreateAsync(Field field, CancellationToken cancellationToken = default)
    {
        _fields.Add(field);
        return Task.FromResult(field);
    }

    public Task<Field> UpdateAsync(Field field, CancellationToken cancellationToken = default)
    {
        var existingFieldIndex = _fields.FindIndex(f => f.FieldId == field.FieldId);
        if (existingFieldIndex >= 0)
        {
            _fields[existingFieldIndex] = field;
        }
        return Task.FromResult(field);
    }

    public Task DeleteAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = _fields.FirstOrDefault(f => f.FieldId == fieldId);
        if (field != null)
        {
            field.Deactivate();
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        var exists = _fields.Any(f => f.FieldId == fieldId);
        return Task.FromResult(exists);
    }

    public Task<bool> CodeExistsInFarmAsync(Guid farmId, string code, Guid? excludeFieldId = null, CancellationToken cancellationToken = default)
    {
        var exists = _fields.Any(f => f.FarmId == farmId && 
                                      string.Equals(f.Code, code, StringComparison.OrdinalIgnoreCase) &&
                                      (excludeFieldId == null || f.FieldId != excludeFieldId));
        return Task.FromResult(exists);
    }
}