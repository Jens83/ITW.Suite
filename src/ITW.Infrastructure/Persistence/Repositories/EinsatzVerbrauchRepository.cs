using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class EinsatzVerbrauchRepository : IEinsatzVerbrauchRepository
{
    private readonly PlatformDbContext _dbContext;

    public EinsatzVerbrauchRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EinsatzVerbrauch>> GetByFahrzeugAsync(
        Lagerort fahrzeug,
        CancellationToken cancellationToken = default)
        => await _dbContext.EinsatzVerbräuche
            .AsNoTracking()
            .Where(v => v.Fahrzeug == fahrzeug)
            .OrderByDescending(v => v.Datum)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EinsatzVerbrauch>> GetByDatumBereichAsync(
        DateOnly von,
        DateOnly bis,
        CancellationToken cancellationToken = default)
        => await _dbContext.EinsatzVerbräuche
            .AsNoTracking()
            .Where(v => v.Datum >= von && v.Datum <= bis)
            .OrderByDescending(v => v.Datum)
            .ToListAsync(cancellationToken);

    public Task<EinsatzVerbrauch?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbContext.EinsatzVerbräuche
            .Include(v => v.Positionen)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task AddAsync(
        EinsatzVerbrauch verbrauch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(verbrauch);
        await _dbContext.EinsatzVerbräuche.AddAsync(verbrauch, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
