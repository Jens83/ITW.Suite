using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class FahrzeugTrackingRepository : IFahrzeugTrackingRepository
{
    private readonly PlatformDbContext _dbContext;

    public FahrzeugTrackingRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<FahrzeugTrackingGeraet>> GetTrackingGeraeteAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FahrzeugTrackingGeraete
            .AsNoTracking()
            .OrderBy(x => x.DeviceIdentifier)
            .ToListAsync(cancellationToken);
    }

    public Task<FahrzeugTrackingGeraet?> GetTrackingGeraetByIdAsync(
        Guid trackingGeraetId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.FahrzeugTrackingGeraete
            .FirstOrDefaultAsync(x => x.Id == trackingGeraetId, cancellationToken);
    }

    public Task<TrackingGeraetEinrichtungscode?> GetAktivenEinrichtungscodeByCodeHashAsync(
    string codeHash,
    DateTimeOffset jetztUtc,
    CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        var hash = codeHash.Trim();
        var jetzt = jetztUtc.ToUniversalTime();

        return _dbContext.TrackingGeraetEinrichtungscodes
            .FirstOrDefaultAsync(
                x =>
                    x.CodeHash == hash &&
                    x.EingeloestAmUtc == null &&
                    x.GueltigBisUtc >= jetzt,
                cancellationToken);
    }

    public Task<FahrzeugTrackingGeraet?> GetTrackingGeraetByDeviceIdentifierAsync(
        string deviceIdentifier,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceIdentifier);

        var normalisiert = deviceIdentifier.Trim();

        return _dbContext.FahrzeugTrackingGeraete
            .FirstOrDefaultAsync(x => x.DeviceIdentifier == normalisiert, cancellationToken);
    }

    public Task<TrackingGeraetStandortAktuell?> GetAktuellenTrackingGeraetStandortAsync(
        Guid trackingGeraetId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TrackingGeraetStandorteAktuell
            .FirstOrDefaultAsync(x => x.TrackingGeraetId == trackingGeraetId, cancellationToken);
    }

    public Task<TrackingGeraetStandortHistorienpunkt?> GetLetztenTrackingGeraetHistorienpunktAsync(
        Guid trackingGeraetId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TrackingGeraetStandortHistorie
            .AsNoTracking()
            .Where(x => x.TrackingGeraetId == trackingGeraetId)
            .OrderByDescending(x => x.ErfasstAmUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TrackingGeraetStandortHistorienpunkt>> GetTrackingGeraetHistorienpunkteAsync(
        Guid trackingGeraetId,
        Guid? routeSessionId,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        var begrenzung = maxCount <= 0 ? 500 : Math.Min(maxCount, 2000);

        var query = _dbContext.TrackingGeraetStandortHistorie
            .AsNoTracking()
            .Where(x => x.TrackingGeraetId == trackingGeraetId);

        if (routeSessionId.HasValue)
        {
            query = query.Where(x => x.RouteSessionId == routeSessionId.Value);
        }

        var punkte = await query
            .OrderByDescending(x => x.ErfasstAmUtc)
            .Take(begrenzung)
            .ToListAsync(cancellationToken);

        return punkte
            .OrderBy(x => x.ErfasstAmUtc)
            .ToList();
    }

    public async Task AddTrackingGeraetAsync(
        FahrzeugTrackingGeraet trackingGeraet,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trackingGeraet);

        await _dbContext.FahrzeugTrackingGeraete.AddAsync(trackingGeraet, cancellationToken);
    }

    public async Task AddEinrichtungscodeAsync(
    TrackingGeraetEinrichtungscode einrichtungscode,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(einrichtungscode);

        await _dbContext.TrackingGeraetEinrichtungscodes.AddAsync(
            einrichtungscode,
            cancellationToken);
    }

    public async Task AddAktuellenTrackingGeraetStandortAsync(
        TrackingGeraetStandortAktuell standort,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(standort);

        await _dbContext.TrackingGeraetStandorteAktuell.AddAsync(standort, cancellationToken);
    }

    public async Task AddTrackingGeraetHistorienpunktAsync(
        TrackingGeraetStandortHistorienpunkt historienpunkt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(historienpunkt);

        await _dbContext.TrackingGeraetStandortHistorie.AddAsync(historienpunkt, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}