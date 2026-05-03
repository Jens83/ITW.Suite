using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class DienstplanPeriodeRepository : IDienstplanPeriodeRepository
{
    private readonly PlatformDbContext _dbContext;

    public DienstplanPeriodeRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<bool> ExistiertAsync(
        int jahr,
        int monat,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DienstplanPerioden
            .AsNoTracking()
            .AnyAsync(
                x => x.Jahr == jahr && x.Monat == monat,
                cancellationToken);
    }

    public async Task AddAsync(
        DienstplanPeriode periode,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(periode);

        await _dbContext.DienstplanPerioden.AddAsync(periode, cancellationToken);
    }

    public Task<DienstplanPeriode?> GetByIdAsync(
        Guid periodeId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DienstplanPerioden
            .FirstOrDefaultAsync(x => x.Id == periodeId, cancellationToken);
    }

    public Task<DienstplanPeriode?> GetAktuelleOffeneAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DienstplanPerioden
            .AsNoTracking()
            .Where(x => x.WunschphaseIstOffen)
            .OrderByDescending(x => x.Jahr)
            .ThenByDescending(x => x.Monat)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountOffeneAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DienstplanPerioden
            .AsNoTracking()
            .CountAsync(x => x.WunschphaseIstOffen, cancellationToken);
    }

    public async Task<IReadOnlyList<DienstplanPeriode>> GetOffeneAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DienstplanPerioden
            .Where(x => x.WunschphaseIstOffen)
            .OrderByDescending(x => x.Jahr)
            .ThenByDescending(x => x.Monat)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DienstplanPeriode>> GetAlleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DienstplanPerioden
            .AsNoTracking()
            .OrderByDescending(x => x.Jahr)
            .ThenByDescending(x => x.Monat)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}