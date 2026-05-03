using ITW.Fahrzeugmanagement.Domain.Entities;

namespace ITW.Fahrzeugmanagement.Application.Contracts;

public interface IFahrzeugTrackingRepository
{
    Task<IReadOnlyList<FahrzeugTrackingGeraet>> GetTrackingGeraeteAsync(
        CancellationToken cancellationToken = default);

    Task<FahrzeugTrackingGeraet?> GetTrackingGeraetByIdAsync(
        Guid trackingGeraetId,
        CancellationToken cancellationToken = default);

    Task<FahrzeugTrackingGeraet?> GetTrackingGeraetByDeviceIdentifierAsync(
        string deviceIdentifier,
        CancellationToken cancellationToken = default);

    Task<TrackingGeraetEinrichtungscode?> GetAktivenEinrichtungscodeByCodeHashAsync(
        string codeHash,
        DateTimeOffset jetztUtc,
        CancellationToken cancellationToken = default);

    Task<TrackingGeraetStandortAktuell?> GetAktuellenTrackingGeraetStandortAsync(
        Guid trackingGeraetId,
        CancellationToken cancellationToken = default);

    Task<TrackingGeraetStandortHistorienpunkt?> GetLetztenTrackingGeraetHistorienpunktAsync(
        Guid trackingGeraetId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrackingGeraetStandortHistorienpunkt>> GetTrackingGeraetHistorienpunkteAsync(
        Guid trackingGeraetId,
        Guid? routeSessionId,
        int maxCount,
        CancellationToken cancellationToken = default);

    Task AddTrackingGeraetAsync(
        FahrzeugTrackingGeraet trackingGeraet,
        CancellationToken cancellationToken = default);

    Task AddEinrichtungscodeAsync(
        TrackingGeraetEinrichtungscode einrichtungscode,
        CancellationToken cancellationToken = default);

    Task AddAktuellenTrackingGeraetStandortAsync(
        TrackingGeraetStandortAktuell standort,
        CancellationToken cancellationToken = default);

    Task AddTrackingGeraetHistorienpunktAsync(
        TrackingGeraetStandortHistorienpunkt historienpunkt,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}