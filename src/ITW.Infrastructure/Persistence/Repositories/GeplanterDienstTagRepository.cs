using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class GeplanterDienstTagRepository : IGeplanterDienstTagRepository
{
    private readonly PlatformDbContext _dbContext;

    public GeplanterDienstTagRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<GeplanterDienstTag?> GetAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.GeplanteDiensttage
            .FirstOrDefaultAsync(
                x => x.DienstplanPeriodeId == dienstplanPeriodeId
                     && x.DienstDatum == dienstDatum,
                cancellationToken);
    }

    public async Task<IReadOnlyList<GeplanterDienstTag>> GetAlleFuerPeriodeAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GeplanteDiensttage
            .AsNoTracking()
            .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
            .OrderBy(x => x.DienstDatum)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        GeplanterDienstTag geplanterDienstTag,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geplanterDienstTag);

        await _dbContext.GeplanteDiensttage.AddAsync(geplanterDienstTag, cancellationToken);
    }

    public void Remove(GeplanterDienstTag geplanterDienstTag)
    {
        ArgumentNullException.ThrowIfNull(geplanterDienstTag);

        _dbContext.GeplanteDiensttage.Remove(geplanterDienstTag);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}