using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class LagerArtikelRepository : ILagerArtikelRepository
{
    private readonly PlatformDbContext _dbContext;

    public LagerArtikelRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LagerArtikel>> GetAlleAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.LagerArtikel
            .AsNoTracking()
            .OrderBy(a => a.Kategorie)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LagerArtikel>> GetAktiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.LagerArtikel
            .AsNoTracking()
            .Where(a => a.IstAktiv)
            .OrderBy(a => a.Kategorie)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LagerArtikel>> GetAktiveByKategorieAsync(
        ArtikelKategorie kategorie,
        CancellationToken cancellationToken = default)
        => await _dbContext.LagerArtikel
            .AsNoTracking()
            .Where(a => a.IstAktiv && a.Kategorie == kategorie)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public Task<LagerArtikel?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbContext.LagerArtikel
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
        => _dbContext.LagerArtikel
            .AnyAsync(a => a.Name == name.Trim(), cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid ausgenommeneId,
        CancellationToken cancellationToken = default)
        => _dbContext.LagerArtikel
            .AnyAsync(a => a.Name == name.Trim() && a.Id != ausgenommeneId, cancellationToken);

    public async Task AddAsync(
        LagerArtikel artikel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(artikel);
        await _dbContext.LagerArtikel.AddAsync(artikel, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
