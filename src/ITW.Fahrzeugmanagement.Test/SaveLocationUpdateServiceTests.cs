using System.Security.Cryptography;
using System.Text;
using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Application.Tracking;
using ITW.Fahrzeugmanagement.Domain.Entities;
using Xunit;

namespace ITW.Fahrzeugmanagement.Test;

public sealed class SaveLocationUpdateServiceTests
{
    [Fact]
    public async Task ExecuteAsync_GibtUnauthorizedWennGeraetNichtExistiert()
    {
        var repository = new FakeFahrzeugTrackingRepository();
        var service = new SaveLocationUpdateService(repository);

        var result = await service.ExecuteAsync(
            new SaveLocationUpdateCommand(
                "SURFACE-ITW-01",
                "geheimer-key",
                53.5571m,
                13.2612m,
                45.5m,
                new DateTimeOffset(2026, 4, 23, 10, 15, 0, TimeSpan.Zero)),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsUnauthorized);
        Assert.Equal("Das Tracking-Gerät ist nicht registriert.", result.ErrorMessage);
        Assert.Equal(0, repository.SaveChangesAufrufe);
        Assert.Empty(repository.Historienpunkte);
        Assert.Null(repository.AktuellerStandort);
    }

    [Fact]
    public async Task ExecuteAsync_SchreibtBeimErstenUpdateAktuellenTabletStandortUndHistorie()
    {
        var deviceIdentifier = "SURFACE-ITW-01";
        var apiKey = "geheimer-key";

        var trackingGeraet = new FahrzeugTrackingGeraet(
            Guid.NewGuid(),
            deviceIdentifier,
            BerechneSha256Hex(apiKey),
            istAktiv: true);

        var repository = new FakeFahrzeugTrackingRepository
        {
            TrackingGeraet = trackingGeraet
        };

        var service = new SaveLocationUpdateService(repository);
        var erfasstAmUtc = new DateTimeOffset(2026, 4, 23, 10, 15, 0, TimeSpan.Zero);

        var result = await service.ExecuteAsync(
            new SaveLocationUpdateCommand(
                deviceIdentifier,
                apiKey,
                53.5571m,
                13.2612m,
                67.5m,
                erfasstAmUtc),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsUnauthorized);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(trackingGeraet.Id, result.TrackingGeraetId);
        Assert.Equal(deviceIdentifier, result.DeviceIdentifier);
        Assert.True(result.RouteSessionId.HasValue);
        Assert.True(result.HistorieGeschrieben);

        Assert.NotNull(repository.AktuellerStandort);
        var aktuellerStandort = repository.AktuellerStandort!;

        Assert.Equal(trackingGeraet.Id, aktuellerStandort.TrackingGeraetId);
        Assert.Equal(result.RouteSessionId, aktuellerStandort.RouteSessionId);
        Assert.Equal(53.5571m, aktuellerStandort.Latitude);
        Assert.Equal(13.2612m, aktuellerStandort.Longitude);
        Assert.Equal(67.5m, aktuellerStandort.SpeedKmh);
        Assert.Equal(erfasstAmUtc, aktuellerStandort.ErfasstAmUtc);
        Assert.Equal(deviceIdentifier, aktuellerStandort.DeviceIdentifier);

        var historienpunkt = Assert.Single(repository.Historienpunkte);
        Assert.Equal(trackingGeraet.Id, historienpunkt.TrackingGeraetId);
        Assert.Equal(result.RouteSessionId, historienpunkt.RouteSessionId);
        Assert.Equal(53.5571m, historienpunkt.Latitude);
        Assert.Equal(13.2612m, historienpunkt.Longitude);
        Assert.Equal(67.5m, historienpunkt.SpeedKmh);
        Assert.Equal(erfasstAmUtc, historienpunkt.ErfasstAmUtc);
        Assert.Equal(deviceIdentifier, historienpunkt.DeviceIdentifier);

        Assert.Equal(1, repository.SaveChangesAufrufe);
        Assert.NotNull(repository.TrackingGeraet!.LetzterKontaktAm);
    }

    [Fact]
    public async Task ExecuteAsync_VerwendetBestehendeRouteSessionUndSchreibtKeineHistorieWennKeineSchwelleErreichtIst()
    {
        var routeSessionId = Guid.NewGuid();
        var deviceIdentifier = "SURFACE-ITW-01";
        var apiKey = "geheimer-key";

        var trackingGeraet = new FahrzeugTrackingGeraet(
            Guid.NewGuid(),
            deviceIdentifier,
            BerechneSha256Hex(apiKey),
            istAktiv: true);

        var repository = new FakeFahrzeugTrackingRepository
        {
            TrackingGeraet = trackingGeraet,
            AktuellerStandort = new TrackingGeraetStandortAktuell(
                trackingGeraet.Id,
                routeSessionId,
                53.557100m,
                13.261200m,
                50m,
                new DateTimeOffset(2026, 4, 23, 10, 00, 0, TimeSpan.Zero),
                deviceIdentifier,
                new DateTimeOffset(2026, 4, 23, 10, 00, 0, TimeSpan.Zero)),
            LetzterHistorienpunkt = new TrackingGeraetStandortHistorienpunkt(
                trackingGeraet.Id,
                routeSessionId,
                53.557100m,
                13.261200m,
                50m,
                new DateTimeOffset(2026, 4, 23, 10, 00, 0, TimeSpan.Zero),
                deviceIdentifier)
        };

        var service = new SaveLocationUpdateService(repository);

        var result = await service.ExecuteAsync(
            new SaveLocationUpdateCommand(
                deviceIdentifier,
                apiKey,
                53.557120m,
                13.261220m,
                55m,
                new DateTimeOffset(2026, 4, 23, 10, 00, 30, TimeSpan.Zero)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(trackingGeraet.Id, result.TrackingGeraetId);
        Assert.Equal(routeSessionId, result.RouteSessionId);
        Assert.False(result.HistorieGeschrieben);

        Assert.NotNull(repository.AktuellerStandort);
        var aktuellerStandort = repository.AktuellerStandort!;

        Assert.Equal(routeSessionId, aktuellerStandort.RouteSessionId);
        Assert.Equal(53.557120m, aktuellerStandort.Latitude);
        Assert.Equal(13.261220m, aktuellerStandort.Longitude);
        Assert.Equal(55m, aktuellerStandort.SpeedKmh);

        Assert.Empty(repository.Historienpunkte);
        Assert.Equal(1, repository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ExecuteAsync_StartetNeueRouteSessionWennLetzterKontaktAelterAls15MinutenIst()
    {
        var alteRouteSessionId = Guid.NewGuid();
        var deviceIdentifier = "SURFACE-ITW-01";
        var apiKey = "geheimer-key";

        var trackingGeraet = new FahrzeugTrackingGeraet(
            Guid.NewGuid(),
            deviceIdentifier,
            BerechneSha256Hex(apiKey),
            istAktiv: true);

        var repository = new FakeFahrzeugTrackingRepository
        {
            TrackingGeraet = trackingGeraet,
            AktuellerStandort = new TrackingGeraetStandortAktuell(
                trackingGeraet.Id,
                alteRouteSessionId,
                53.557100m,
                13.261200m,
                30m,
                new DateTimeOffset(2026, 4, 23, 10, 00, 0, TimeSpan.Zero),
                deviceIdentifier,
                new DateTimeOffset(2026, 4, 23, 10, 00, 0, TimeSpan.Zero)),
            LetzterHistorienpunkt = new TrackingGeraetStandortHistorienpunkt(
                trackingGeraet.Id,
                alteRouteSessionId,
                53.557100m,
                13.261200m,
                30m,
                new DateTimeOffset(2026, 4, 23, 10, 00, 0, TimeSpan.Zero),
                deviceIdentifier)
        };

        var service = new SaveLocationUpdateService(repository);

        var result = await service.ExecuteAsync(
            new SaveLocationUpdateCommand(
                deviceIdentifier,
                apiKey,
                53.560000m,
                13.265000m,
                35m,
                new DateTimeOffset(2026, 4, 23, 10, 16, 0, TimeSpan.Zero)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.RouteSessionId.HasValue);
        Assert.NotEqual(alteRouteSessionId, result.RouteSessionId.Value);
        Assert.True(result.HistorieGeschrieben);

        var historienpunkt = Assert.Single(repository.Historienpunkte);
        Assert.Equal(result.RouteSessionId, historienpunkt.RouteSessionId);
    }

    private static string BerechneSha256Hex(string wert)
    {
        var bytes = Encoding.UTF8.GetBytes(wert);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private sealed class FakeFahrzeugTrackingRepository : IFahrzeugTrackingRepository
    {
        public FahrzeugTrackingGeraet? TrackingGeraet { get; set; }

        public TrackingGeraetStandortAktuell? AktuellerStandort { get; set; }

        public TrackingGeraetStandortHistorienpunkt? LetzterHistorienpunkt { get; set; }

        public List<TrackingGeraetStandortHistorienpunkt> Historienpunkte { get; } = [];

        public int SaveChangesAufrufe { get; private set; }

        public Task<IReadOnlyList<FahrzeugTrackingGeraet>> GetTrackingGeraeteAsync(
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<FahrzeugTrackingGeraet> result = TrackingGeraet is null
                ? []
                : [TrackingGeraet];

            return Task.FromResult(result);
        }

        public Task<FahrzeugTrackingGeraet?> GetTrackingGeraetByIdAsync(
            Guid trackingGeraetId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                TrackingGeraet is not null && TrackingGeraet.Id == trackingGeraetId
                    ? TrackingGeraet
                    : null);
        }

        public Task<FahrzeugTrackingGeraet?> GetTrackingGeraetByDeviceIdentifierAsync(
            string deviceIdentifier,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                TrackingGeraet is not null && TrackingGeraet.DeviceIdentifier == deviceIdentifier
                    ? TrackingGeraet
                    : null);
        }

        public Task<TrackingGeraetStandortAktuell?> GetAktuellenTrackingGeraetStandortAsync(
            Guid trackingGeraetId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                AktuellerStandort is not null && AktuellerStandort.TrackingGeraetId == trackingGeraetId
                    ? AktuellerStandort
                    : null);
        }

        public Task<TrackingGeraetStandortHistorienpunkt?> GetLetztenTrackingGeraetHistorienpunktAsync(
            Guid trackingGeraetId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                LetzterHistorienpunkt is not null && LetzterHistorienpunkt.TrackingGeraetId == trackingGeraetId
                    ? LetzterHistorienpunkt
                    : null);
        }

        public Task<IReadOnlyList<TrackingGeraetStandortHistorienpunkt>> GetTrackingGeraetHistorienpunkteAsync(
    Guid trackingGeraetId,
    Guid? routeSessionId,
    int maxCount,
    CancellationToken cancellationToken = default)
        {
            var query = Historienpunkte
                .Where(x => x.TrackingGeraetId == trackingGeraetId);

            if (routeSessionId.HasValue)
            {
                query = query.Where(x => x.RouteSessionId == routeSessionId.Value).ToList();
            }

            IReadOnlyList<TrackingGeraetStandortHistorienpunkt> result = query
                .OrderBy(x => x.ErfasstAmUtc)
                .Take(maxCount)
                .ToList();

            return Task.FromResult(result);
        }

        public Task AddTrackingGeraetAsync(
            FahrzeugTrackingGeraet trackingGeraet,
            CancellationToken cancellationToken = default)
        {
            TrackingGeraet = trackingGeraet;
            return Task.CompletedTask;
        }

        // Stub – wird im aktuellen Test nicht beansprucht.
        public Task<TrackingGeraetEinrichtungscode?> GetAktivenEinrichtungscodeByCodeHashAsync(
            string codeHash,
            DateTimeOffset jetztUtc,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TrackingGeraetEinrichtungscode?>(null);
        }

        // Stub – wird im aktuellen Test nicht beansprucht.
        public Task AddEinrichtungscodeAsync(
            TrackingGeraetEinrichtungscode einrichtungscode,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task AddAktuellenTrackingGeraetStandortAsync(
            TrackingGeraetStandortAktuell standort,
            CancellationToken cancellationToken = default)
        {
            AktuellerStandort = standort;
            return Task.CompletedTask;
        }

        public Task AddTrackingGeraetHistorienpunktAsync(
            TrackingGeraetStandortHistorienpunkt historienpunkt,
            CancellationToken cancellationToken = default)
        {
            Historienpunkte.Add(historienpunkt);
            LetzterHistorienpunkt = historienpunkt;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAufrufe++;
            return Task.CompletedTask;
        }
    }
}