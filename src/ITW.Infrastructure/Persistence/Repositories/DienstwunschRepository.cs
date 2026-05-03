using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class DienstwunschRepository : IDienstwunschRepository
{
    private readonly PlatformDbContext _dbContext;

    public DienstwunschRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<Dienstwunsch?> GetAsync(
        Guid dienstplanPeriodeId,
        string userId,
        DateOnly wunschDatum,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DienstplanWuensche
            .FirstOrDefaultAsync(
                x => x.DienstplanPeriodeId == dienstplanPeriodeId
                     && x.UserId == userId
                     && x.WunschDatum == wunschDatum,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerBenutzerAsync(
        Guid dienstplanPeriodeId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DienstplanWuensche
            .AsNoTracking()
            .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId && x.UserId == userId)
            .OrderBy(x => x.WunschDatum)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerPeriodeAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default)
    {
        if (dienstplanPeriodeId == Guid.Empty)
        {
            return Array.Empty<Dienstwunsch>();
        }

        return await _dbContext.DienstplanWuensche
            .AsNoTracking()
            .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
            .OrderBy(x => x.WunschDatum)
            .ThenBy(x => x.ErstelltAm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerTagAsync(
        Guid dienstplanPeriodeId,
        DateOnly wunschDatum,
        DienstwunschTyp? wunschTyp = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DienstplanWuensche
            .AsNoTracking()
            .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId && x.WunschDatum == wunschDatum);

        if (wunschTyp.HasValue)
        {
            query = query.Where(x => x.WunschTyp == wunschTyp.Value);
        }

        return await query
            .OrderBy(x => x.ErstelltAm)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Dienstwunsch dienstwunsch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dienstwunsch);

        await _dbContext.DienstplanWuensche.AddAsync(dienstwunsch, cancellationToken);
    }

    public void Remove(Dienstwunsch dienstwunsch)
    {
        ArgumentNullException.ThrowIfNull(dienstwunsch);

        _dbContext.DienstplanWuensche.Remove(dienstwunsch);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}