using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Domain.Entities;
using FarmRegistry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FarmRegistry.Infrastructure.Repositories;

public sealed class FarmRepository : IFarmRepository
{
    private readonly FarmRegistryDbContext _context;

    public FarmRepository(FarmRegistryDbContext context)
    {
        _context = context;
    }

    public async Task<Farm?> GetByIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        return await _context.Farms
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.FarmId == farmId, cancellationToken);
    }

    public async Task<IEnumerable<Farm>> GetAllAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        // TODO: Filtrar por ownerId quando a entidade Farm incluir essa propriedade
        return await _context.Farms
            .Include(f => f.Fields)
            .Where(f => f.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Farm> CreateAsync(Farm farm, CancellationToken cancellationToken = default)
    {
        await _context.Farms.AddAsync(farm, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return farm;
    }

    public async Task<Farm> UpdateAsync(Farm farm, CancellationToken cancellationToken = default)
    {
        _context.Farms.Update(farm);
        await _context.SaveChangesAsync(cancellationToken);
        return farm;
    }

    public async Task DeleteAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = await GetByIdAsync(farmId, cancellationToken);
        if (farm != null)
        {
            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        return await _context.Farms.AnyAsync(f => f.FarmId == farmId, cancellationToken);
    }
}