using ITW.Infrastructure.Persistence.DbContexts;
using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class SauerstoffFlascheRepository : ISauerstoffFlascheRepository
{
    private readonly PlatformDbContext _dbContext;

    public SauerstoffFlascheRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SauerstoffFlasche>> GetAktiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.SauerstoffFlaschen
            .AsNoTracking()
            .Where(f => f.IstAktiv)
            .OrderBy(f => f.FahrzeugId == null ? 0 : 1)
            .ThenBy(f => f.Status)
            .ThenBy(f => f.Groesse)
            .ToListAsync(cancellationToken);

    public Task<SauerstoffFlasche?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbContext.SauerstoffFlaschen
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<SauerstoffFlasche?> GetByFlaschenNummerAsync(
        string flaschenNummer,
        CancellationToken cancellationToken = default)
        => _dbContext.SauerstoffFlaschen
            .FirstOrDefaultAsync(
                f => f.IstAktiv && f.FlaschenNummer == flaschenNummer.Trim(),
                cancellationToken);

    public async Task<IReadOnlyList<SauerstoffFlasche>> GetByLieferungIdAsync(
        Guid lieferungId,
        CancellationToken cancellationToken = default)
        => await _dbContext.SauerstoffFlaschen
            .AsNoTracking()
            .Where(f => f.LieferungId == lieferungId)
            .OrderBy(f => f.Groesse)
            .ThenBy(f => f.FlaschenNummer)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByFlaschenNummerAsync(
        string flaschenNummer,
        CancellationToken cancellationToken = default)
        => _dbContext.SauerstoffFlaschen
            .AnyAsync(f => f.IstAktiv && f.FlaschenNummer == flaschenNummer.Trim(), cancellationToken);

    public async Task AddAsync(
        SauerstoffFlasche flasche,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(flasche);
        await _dbContext.SauerstoffFlaschen.AddAsync(flasche, cancellationToken);
    }

    public async Task AddBewegungAsync(
        SauerstoffBewegung bewegung,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bewegung);
        await _dbContext.SauerstoffBewegungen.AddAsync(bewegung, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
