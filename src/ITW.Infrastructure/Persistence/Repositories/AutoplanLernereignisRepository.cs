using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class AutoplanLernereignisRepository : IAutoplanLernereignisRepository
{
    private readonly PlatformDbContext _dbContext;

    public AutoplanLernereignisRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(
        AutoplanLernereignis lernereignis,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lernereignis);

        await _dbContext.AutoplanLernereignisse.AddAsync(lernereignis, cancellationToken);
    }

    public async Task<IReadOnlyList<AutoplanLernereignis>> GetVertretungsLernereignisseAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AutoplanLernereignisse
            .AsNoTracking()
            .Where(x => x.EreignisTypCode == AutoplanLernereignisTypCode.VertretungManuellGeaendert)
            .OrderByDescending(x => x.ErfasstAm)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AutoplanLernereignis>> GetGrundbesetzungsLernereignisseAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AutoplanLernereignisse
            .AsNoTracking()
            .Where(x =>
                x.EreignisTypCode == AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert ||
                x.EreignisTypCode == AutoplanLernereignisTypCode.AutoplanVorschlagBestaetigt)
            .OrderByDescending(x => x.ErfasstAm)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}