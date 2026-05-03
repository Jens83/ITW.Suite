using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class FahrtenbuchRepository : IFahrtenbuchRepository
{
    private readonly PlatformDbContext _dbContext;

    public FahrtenbuchRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<FahrtenbuchEintrag>> GetByFahrzeugIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return [];
        }

        return await _dbContext.FahrtenbuchEintraege
            .AsNoTracking()
            .Where(x => x.FahrzeugId == fahrzeugId)
            .OrderByDescending(x => x.StartzeitUtc)
            .ThenByDescending(x => x.ErstelltAm)
            .ToListAsync(cancellationToken);
    }

    public Task<FahrtenbuchEintrag?> GetByIdAsync(
        Guid eintragId,
        CancellationToken cancellationToken = default)
    {
        if (eintragId == Guid.Empty)
        {
            return Task.FromResult<FahrtenbuchEintrag?>(null);
        }

        return _dbContext.FahrtenbuchEintraege
            .FirstOrDefaultAsync(x => x.Id == eintragId, cancellationToken);
    }

    public async Task AddAsync(
        FahrtenbuchEintrag eintrag,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eintrag);

        await _dbContext.FahrtenbuchEintraege.AddAsync(eintrag, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}