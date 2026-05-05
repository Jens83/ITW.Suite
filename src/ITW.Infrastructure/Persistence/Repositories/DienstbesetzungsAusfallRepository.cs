using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class DienstbesetzungsAusfallRepository : IDienstbesetzungsAusfallRepository
{
    private readonly PlatformDbContext _dbContext;

    public DienstbesetzungsAusfallRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public Task<GeplanterDienstTagAusfall?> GetAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        DienstbesetzungsSlotCode besetzungsSlotCode,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.GeplanteDiensttagAusfaelle
            .FirstOrDefaultAsync(
                x => x.DienstplanPeriodeId == dienstplanPeriodeId
                     && x.DienstDatum == dienstDatum
                     && x.BesetzungsSlotCode == besetzungsSlotCode,
                cancellationToken);
    }

    public async Task<IReadOnlyList<GeplanterDienstTagAusfall>> GetAlleFuerTagAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GeplanteDiensttagAusfaelle
            .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId && x.DienstDatum == dienstDatum)
            .OrderBy(x => x.BesetzungsSlotCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GeplanterDienstTagAusfall>> GetAlleFuerPeriodeAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GeplanteDiensttagAusfaelle
            .AsNoTracking()
            .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
            .OrderBy(x => x.DienstDatum)
            .ThenBy(x => x.BesetzungsSlotCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DateOnly>> GetUrlaubstageFuerBenutzerUndJahrAsync(
        string userId,
        int jahr,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<DateOnly>();
        }

        return await _dbContext.GeplanteDiensttagAusfaelle
            .AsNoTracking()
            .Where(x =>
                x.UrspruenglichGeplanterUserId == userId &&
                x.AusfallGrundCode == DienstausfallGrundCode.Urlaub &&
                x.DienstDatum.Year == jahr)
            .Select(x => x.DienstDatum)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        GeplanterDienstTagAusfall ausfall,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ausfall);

        await _dbContext.GeplanteDiensttagAusfaelle.AddAsync(ausfall, cancellationToken);
    }

    public void Remove(GeplanterDienstTagAusfall ausfall)
    {
        ArgumentNullException.ThrowIfNull(ausfall);

        _dbContext.GeplanteDiensttagAusfaelle.Remove(ausfall);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}