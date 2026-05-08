using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class ArtikelChargeRepository : IArtikelChargeRepository
{
    private readonly PlatformDbContext _dbContext;

    public ArtikelChargeRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ArtikelCharge>> GetAktiveByArtikelAsync(
        Guid artikelId,
        CancellationToken cancellationToken = default)
        => await _dbContext.ArtikelChargen
            .AsNoTracking()
            .Where(c => c.ArtikelId == artikelId && !c.IstAusgebucht)
            .OrderBy(c => c.Ablaufdatum)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ArtikelCharge>> GetAktiveByArtikelUndLagerortAsync(
        Guid artikelId,
        Lagerort lagerort,
        CancellationToken cancellationToken = default)
        => await _dbContext.ArtikelChargen
            .Where(c => c.ArtikelId == artikelId && c.Lagerort == lagerort && !c.IstAusgebucht)
            .OrderBy(c => c.Ablaufdatum)
            .ThenBy(c => c.EingebuchtAm)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ArtikelCharge>> GetAktiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.ArtikelChargen
            .AsNoTracking()
            .Where(c => !c.IstAusgebucht)
            .OrderBy(c => c.Ablaufdatum)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ArtikelCharge>> GetBaldAblaufendeAsync(
        DateOnly heute,
        int vorwarnungTage,
        CancellationToken cancellationToken = default)
    {
        var grenze = heute.AddDays(vorwarnungTage);
        return await _dbContext.ArtikelChargen
            .AsNoTracking()
            .Where(c => !c.IstAusgebucht && c.Ablaufdatum >= heute && c.Ablaufdatum <= grenze)
            .OrderBy(c => c.Ablaufdatum)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ArtikelCharge>> GetAbgelaufeneAsync(
        DateOnly heute,
        CancellationToken cancellationToken = default)
        => await _dbContext.ArtikelChargen
            .AsNoTracking()
            .Where(c => !c.IstAusgebucht && c.Ablaufdatum < heute)
            .OrderBy(c => c.Ablaufdatum)
            .ToListAsync(cancellationToken);

    public Task<ArtikelCharge?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbContext.ArtikelChargen
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(
        ArtikelCharge charge,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(charge);
        await _dbContext.ArtikelChargen.AddAsync(charge, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
