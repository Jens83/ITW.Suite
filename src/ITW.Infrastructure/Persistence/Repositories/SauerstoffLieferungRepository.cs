using ITW.Infrastructure.Persistence.DbContexts;
using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class SauerstoffLieferungRepository : ISauerstoffLieferungRepository
{
    private readonly PlatformDbContext _dbContext;

    public SauerstoffLieferungRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SauerstoffLieferung>> GetAlleAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.SauerstoffLieferungen
            .AsNoTracking()
            .OrderByDescending(l => l.Lieferdatum)
            .ThenByDescending(l => l.ErfasstAm)
            .ToListAsync(cancellationToken);

    public Task<SauerstoffLieferung?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbContext.SauerstoffLieferungen
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<bool> ExistsByLieferscheinNummerAsync(
        string lieferscheinNummer,
        CancellationToken cancellationToken = default)
        => _dbContext.SauerstoffLieferungen
            .AnyAsync(l => l.LieferscheinNummer == lieferscheinNummer.Trim(), cancellationToken);

    public async Task AddAsync(
        SauerstoffLieferung lieferung,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lieferung);
        await _dbContext.SauerstoffLieferungen.AddAsync(lieferung, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
