using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class FahrzeugRepository : IFahrzeugRepository
{
    private readonly PlatformDbContext _dbContext;

    public FahrzeugRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<Fahrzeug>> GetFahrzeugeAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Fahrzeuge
            .AsNoTracking()
            .OrderBy(x => x.InterneNummer)
            .ThenBy(x => x.Kennzeichen)
            .ToListAsync(cancellationToken);
    }

    public Task<Fahrzeug?> GetFahrzeugByIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return Task.FromResult<Fahrzeug?>(null);
        }

        return _dbContext.Fahrzeuge
            .FirstOrDefaultAsync(x => x.Id == fahrzeugId, cancellationToken);
    }

    public Task<bool> ExistsByKennzeichenAsync(
        string kennzeichen,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kennzeichen);

        var normalisiert = kennzeichen.Trim().ToUpperInvariant();

        return _dbContext.Fahrzeuge
            .AnyAsync(x => x.Kennzeichen == normalisiert, cancellationToken);
    }

    public Task<bool> ExistsByKennzeichenAsync(
        string kennzeichen,
        Guid ausgenommeneFahrzeugId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kennzeichen);

        var normalisiert = kennzeichen.Trim().ToUpperInvariant();

        return _dbContext.Fahrzeuge
            .AnyAsync(
                x => x.Kennzeichen == normalisiert &&
                     x.Id != ausgenommeneFahrzeugId,
                cancellationToken);
    }

    public Task<bool> ExistsByVinAsync(
        string vin,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vin);

        var normalisiert = vin.Trim().ToUpperInvariant();

        return _dbContext.Fahrzeuge
            .AnyAsync(x => x.Vin == normalisiert, cancellationToken);
    }

    public Task<bool> ExistsByVinAsync(
        string vin,
        Guid ausgenommeneFahrzeugId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vin);

        var normalisiert = vin.Trim().ToUpperInvariant();

        return _dbContext.Fahrzeuge
            .AnyAsync(
                x => x.Vin == normalisiert &&
                     x.Id != ausgenommeneFahrzeugId,
                cancellationToken);
    }

    public async Task AddFahrzeugAsync(
        Fahrzeug fahrzeug,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fahrzeug);

        await _dbContext.Fahrzeuge.AddAsync(fahrzeug, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}