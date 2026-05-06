using ITW.Application.Aktivitaet;
using ITW.Application.Organisation.Contracts;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class AktivitaetsLogRepository : IAktivitaetsLogRepository
{
    private readonly PlatformDbContext _dbContext;

    public AktivitaetsLogRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AktivitaetsEintrag>> GetByBereichAsync(
        OrganisationsbereichCode bereich,
        int maxAnzahl = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AktivitaetsLog
            .AsNoTracking()
            .Where(x => x.Bereich == bereich)
            .OrderByDescending(x => x.Zeitpunkt)
            .Take(maxAnzahl)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistiertAktivitaetAsync(
        OrganisationsbereichCode bereich,
        string text,
        DateTimeOffset seit,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AktivitaetsLog
            .AnyAsync(x =>
                x.Bereich == bereich &&
                x.Text == text &&
                x.Zeitpunkt >= seit,
                cancellationToken);
    }

    public async Task AddAsync(
        AktivitaetsEintrag eintrag,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eintrag);
        await _dbContext.AktivitaetsLog.AddAsync(eintrag, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
