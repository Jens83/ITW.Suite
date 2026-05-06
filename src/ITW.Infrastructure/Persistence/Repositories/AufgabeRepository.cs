using ITW.Application.Aufgaben;
using ITW.Application.Organisation.Contracts;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class AufgabeRepository : IAufgabeRepository
{
    private readonly PlatformDbContext _dbContext;

    public AufgabeRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Aufgabe>> GetOffeneByBereichAsync(
        OrganisationsbereichCode bereich,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Aufgaben
            .Where(x => x.Bereich == bereich && x.Status == AufgabeStatus.Offen)
            .OrderByDescending(x => x.Prioritaet)
            .ThenBy(x => x.Faelligkeitsdatum)
            .ThenBy(x => x.ErstelltAm)
            .ToListAsync(cancellationToken);
    }

    public async Task<Aufgabe?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Aufgaben
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistiertOffeneSystemaufgabeAsync(
        string systemSchluessel,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Aufgaben
            .AnyAsync(x =>
                x.SystemSchluessel == systemSchluessel &&
                x.Status == AufgabeStatus.Offen,
                cancellationToken);
    }

    public async Task AddAsync(
        Aufgabe aufgabe,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aufgabe);
        await _dbContext.Aufgaben.AddAsync(aufgabe, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
