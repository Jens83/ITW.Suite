using ITW.Application.Organisation.Contracts;
using ITW.Domain.Organisation.Entities;
using ITW.Domain.Organisation.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class ModulZuweisungRepository : IModulZuweisungRepository
{
    private readonly PlatformDbContext _dbContext;

    public ModulZuweisungRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<ModulZuweisung?> GetByModulBereichRolleAsync(
        Modul modul,
        Organisationsbereich bereich,
        Bereichsrolle rolle,
        CancellationToken cancellationToken = default)
    {
        if (modul == Modul.Unbekannt ||
            bereich == Organisationsbereich.Unbekannt ||
            rolle == Bereichsrolle.Unbekannt)
        {
            return null;
        }

        return await _dbContext.ModulZuweisungen
            .FirstOrDefaultAsync(
                x => x.Modul == modul &&
                     x.Bereich == bereich &&
                     x.Rolle == rolle,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ModulZuweisung>> GetAktiveModuleFuerBereichUndRolleAsync(
        Organisationsbereich bereich,
        Bereichsrolle rolle,
        CancellationToken cancellationToken = default)
    {
        if (bereich == Organisationsbereich.Unbekannt ||
            rolle == Bereichsrolle.Unbekannt)
        {
            return Array.Empty<ModulZuweisung>();
        }

        return await _dbContext.ModulZuweisungen
            .AsNoTracking()
            .Where(x => x.Bereich == bereich &&
                        x.Rolle == rolle &&
                        x.IstAktiv)
            .OrderBy(x => x.Modul)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ModulZuweisung>> GetAlleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ModulZuweisungen
            .AsNoTracking()
            .OrderBy(x => x.Bereich)
            .ThenBy(x => x.Rolle)
            .ThenBy(x => x.Modul)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        ModulZuweisung zuweisung,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(zuweisung);

        await _dbContext.ModulZuweisungen.AddAsync(zuweisung, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}