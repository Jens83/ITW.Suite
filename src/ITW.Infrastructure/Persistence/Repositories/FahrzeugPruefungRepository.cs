using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Fahrzeugmanagement.Domain.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class FahrzeugPruefungRepository : IFahrzeugPruefungRepository
{
    private readonly PlatformDbContext _dbContext;

    public FahrzeugPruefungRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<FahrzeugPruefung>> GetByFahrzeugIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return [];
        }

        return await _dbContext.FahrzeugPruefungen
            .AsNoTracking()
            .Where(x => x.FahrzeugId == fahrzeugId)
            .OrderBy(x => x.FaelligAm)
            .ToListAsync(cancellationToken);
    }

    public Task<FahrzeugPruefung?> GetByFahrzeugIdUndTypAsync(
        Guid fahrzeugId,
        FahrzeugPruefungTyp typ,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty || typ == FahrzeugPruefungTyp.Unbekannt)
        {
            return Task.FromResult<FahrzeugPruefung?>(null);
        }

        return _dbContext.FahrzeugPruefungen
            .FirstOrDefaultAsync(
                x => x.FahrzeugId == fahrzeugId &&
                     x.Typ == typ,
                cancellationToken);
    }

    public async Task AddAsync(
        FahrzeugPruefung pruefung,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pruefung);

        await _dbContext.FahrzeugPruefungen.AddAsync(pruefung, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}