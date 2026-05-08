using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class ArtikelBestandRepository : IArtikelBestandRepository
{
    private readonly PlatformDbContext _dbContext;

    public ArtikelBestandRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ArtikelBestand>> GetAlleAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.ArtikelBestaende
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ArtikelBestand>> GetByLagerortAsync(
        Lagerort lagerort,
        CancellationToken cancellationToken = default)
        => await _dbContext.ArtikelBestaende
            .AsNoTracking()
            .Where(b => b.Lagerort == lagerort)
            .ToListAsync(cancellationToken);

    public Task<ArtikelBestand?> GetByArtikelUndLagerortAsync(
        Guid artikelId,
        Lagerort lagerort,
        CancellationToken cancellationToken = default)
        => _dbContext.ArtikelBestaende
            .FirstOrDefaultAsync(b => b.ArtikelId == artikelId && b.Lagerort == lagerort, cancellationToken);

    public async Task<IReadOnlyList<ArtikelBestand>> GetUnterschrittenerMindestbestandAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.ArtikelBestaende
            .AsNoTracking()
            .Join(
                _dbContext.LagerArtikel.Where(a => a.IstAktiv),
                b => b.ArtikelId,
                a => a.Id,
                (b, a) => new { Bestand = b, Artikel = a })
            .Where(x => x.Bestand.Menge < x.Artikel.Mindestbestand && x.Artikel.Mindestbestand > 0)
            .Select(x => x.Bestand)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        ArtikelBestand bestand,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bestand);
        await _dbContext.ArtikelBestaende.AddAsync(bestand, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
