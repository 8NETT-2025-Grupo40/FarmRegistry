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

    private IQueryable<Field> QueryByOwner(Guid ownerId)
    {
        return _context.Fields.Where(field =>
            _context.Farms.Any(farm => farm.FarmId == field.FarmId && farm.OwnerId == ownerId));
    }

    private IQueryable<Field> QueryByOwnerWithBoundaryPoints(Guid ownerId)
    {
        return QueryByOwner(ownerId)
            .Include(field => field.BoundaryPoints);
    }

    public async Task<Field?> GetByIdAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        return await QueryByOwnerWithBoundaryPoints(ownerId)
            .FirstOrDefaultAsync(f => f.FieldId == fieldId, cancellationToken);
    }

    public async Task<IEnumerable<Field>> GetByFarmIdAsync(Guid ownerId, Guid farmId, CancellationToken cancellationToken = default)
    {
        return await QueryByOwnerWithBoundaryPoints(ownerId)
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

    public async Task DeleteAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        var field = await GetByIdAsync(ownerId, fieldId, cancellationToken);
        if (field != null)
        {
            _context.Fields.Remove(field);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid ownerId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        return await QueryByOwner(ownerId).AnyAsync(f => f.FieldId == fieldId, cancellationToken);
    }

    public async Task<bool> CodeExistsInFarmAsync(Guid ownerId, Guid farmId, string code, Guid? excludeFieldId = null, CancellationToken cancellationToken = default)
    {
        return await QueryByOwner(ownerId).AnyAsync(f =>
            f.FarmId == farmId &&
            f.Code.ToLower() == code.ToLower() &&
            (excludeFieldId == null || f.FieldId != excludeFieldId),
            cancellationToken);
    }
}
