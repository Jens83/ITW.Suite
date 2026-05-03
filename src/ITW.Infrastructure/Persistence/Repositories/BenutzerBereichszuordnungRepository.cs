using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Entities;
using ITW.Domain.Organisation.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class BenutzerBereichszuordnungRepository : IBenutzerBereichszuordnungRepository
{
    private readonly PlatformDbContext _dbContext;

    public BenutzerBereichszuordnungRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<BenutzerBereichszuordnung?> GetAktivePrimaereZuordnungAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _dbContext.BenutzerBereichszuordnungen
            .FirstOrDefaultAsync(
                x => x.UserId == userId &&
                     x.IsActive &&
                     x.IsPrimary,
                cancellationToken);
    }

    public async Task<IReadOnlyList<BenutzerBereichszuordnung>> GetAktivePrimaereZuordnungenByBereichAsync(
        Organisationsbereich bereich,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BenutzerBereichszuordnungen
            .AsNoTracking()
            .Where(x => x.Bereich == bereich &&
                        x.IsActive &&
                        x.IsPrimary)
            .OrderBy(x => x.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        BenutzerBereichszuordnung zuordnung,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(zuordnung);

        await _dbContext.BenutzerBereichszuordnungen.AddAsync(zuordnung, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}