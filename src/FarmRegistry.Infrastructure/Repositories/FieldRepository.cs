using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Entities;
using FarmRegistry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FarmRegistry.Infrastructure.Repositories;

public sealed class FieldRepository : IFieldRepository
{
    private readonly FarmRegistryDbContext _context;

    public FieldRepository(FarmRegistryDbContext context)
    {
        _context = context;
    }

    public async Task<Field?> GetByIdAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        return await _context.Fields
            .FirstOrDefaultAsync(f => f.FieldId == fieldId, cancellationToken);
    }

    public async Task<IEnumerable<Field>> GetByFarmIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        return await _context.Fields
            .Where(f => f.FarmId == farmId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Field> CreateAsync(Field field, CancellationToken cancellationToken = default)
    {
        await _context.Fields.AddAsync(field, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return field;
    }

    public async Task<Field> UpdateAsync(Field field, CancellationToken cancellationToken = default)
    {
        _context.Fields.Update(field);
        await _context.SaveChangesAsync(cancellationToken);
        return field;
    }

    public async Task DeleteAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await GetByIdAsync(fieldId, cancellationToken);
        if (field != null)
        {
            _context.Fields.Remove(field);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        return await _context.Fields.AnyAsync(f => f.FieldId == fieldId, cancellationToken);
    }

    public async Task<bool> CodeExistsInFarmAsync(Guid farmId, string code, Guid? excludeFieldId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Fields.AnyAsync(f => 
            f.FarmId == farmId && 
            f.Code.ToLower() == code.ToLower() &&
            (excludeFieldId == null || f.FieldId != excludeFieldId), 
            cancellationToken);
    }
}