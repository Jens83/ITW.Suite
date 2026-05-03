using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class FahrzeugVertragRepository : IFahrzeugVertragRepository
{
    private readonly PlatformDbContext _dbContext;

    public FahrzeugVertragRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<FahrzeugVertrag>> GetByFahrzeugIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return [];
        }

        return await _dbContext.FahrzeugVertraege
            .AsNoTracking()
            .Where(x => x.FahrzeugId == fahrzeugId)
            .OrderBy(x => x.GueltigBis ?? DateOnly.MaxValue)
            .ThenBy(x => x.Anbieter)
            .ToListAsync(cancellationToken);
    }

    public Task<FahrzeugVertrag?> GetByIdAsync(
        Guid vertragId,
        CancellationToken cancellationToken = default)
    {
        if (vertragId == Guid.Empty)
        {
            return Task.FromResult<FahrzeugVertrag?>(null);
        }

        return _dbContext.FahrzeugVertraege
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == vertragId, cancellationToken);
    }

    public async Task AddAsync(
        FahrzeugVertrag vertrag,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vertrag);

        await _dbContext.FahrzeugVertraege.AddAsync(vertrag, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid vertragId,
        CancellationToken cancellationToken = default)
    {
        if (vertragId == Guid.Empty)
        {
            return;
        }

        var vertrag = await _dbContext.FahrzeugVertraege
            .FirstOrDefaultAsync(x => x.Id == vertragId, cancellationToken);

        if (vertrag is null)
        {
            return;
        }

        _dbContext.FahrzeugVertraege.Remove(vertrag);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}