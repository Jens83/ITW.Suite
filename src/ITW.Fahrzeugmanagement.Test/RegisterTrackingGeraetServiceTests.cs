using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Application.Tracking;
using ITW.Fahrzeugmanagement.Domain.Entities;
using Xunit;

namespace ITW.Fahrzeugmanagement.Test;

public sealed class RegisterTrackingGeraetServiceTests
{
    [Fact]
    public async Task ExecuteAsync_GibtFehlerWennDeviceIdentifierLeerIst()
    {
        var repository = new FakeFahrzeugTrackingRepository();
        var service = new RegisterTrackingGeraetService(repository);

        var result = await service.ExecuteAsync(
            new RegisterTrackingGeraetCommand(string.Empty),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Der Device-Identifier ist erforderlich.", result.ErrorMessage);
        Assert.Equal(0, repository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ExecuteAsync_LegtNeuesTrackingGeraetAnUndLiefertApiKeyZurueck()
    {
        var repository = new FakeFahrzeugTrackingRepository();
        var service = new RegisterTrackingGeraetService(repository);

        var result = await service.ExecuteAsync(
            new RegisterTrackingGeraetCommand("SURFACE-ITW-01"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.TrackingGeraetId);
        Assert.Equal("SURFACE-ITW-01", result.DeviceIdentifier);
        Assert.False(string.IsNullOrWhiteSpace(result.ApiKey));

        var trackingGeraet = Assert.Single(repository.HinzugefuegteTrackingGeraete);
        Assert.Equal("SURFACE-ITW-01", trackingGeraet.DeviceIdentifier);
        Assert.True(trackingGeraet.IstAktiv);
        Assert.False(string.IsNullOrWhiteSpace(trackingGeraet.ApiKeyHash));
        Assert.Equal(1, repository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ExecuteAsync_AktualisiertVorhandenesGeraetMitNeuemApiKeyUndAktiviertEs()
    {
        var vorhandenesGeraet = new FahrzeugTrackingGeraet(
            Guid.NewGuid(),
            "SURFACE-ITW-01",
            "ALTERHASH",
            istAktiv: false);

        var repository = new FakeFahrzeugTrackingRepository
        {
            TrackingGeraet = vorhandenesGeraet
        };

        var service = new RegisterTrackingGeraetService(repository);

        var result = await service.ExecuteAsync(
            new RegisterTrackingGeraetCommand("SURFACE-ITW-01"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(vorhandenesGeraet.Id, result.TrackingGeraetId);
        Assert.True(vorhandenesGeraet.IstAktiv);
        Assert.NotEqual("ALTERHASH", vorhandenesGeraet.ApiKeyHash);
        Assert.Empty(repository.HinzugefuegteTrackingGeraete);
        Assert.Equal(1, repository.SaveChangesAufrufe);
    }

    private sealed class FakeFahrzeugTrackingRepository : IFahrzeugTrackingRepository
    {
        public FahrzeugTrackingGeraet? TrackingGeraet { get; set; }

        public List<FahrzeugTrackingGeraet> HinzugefuegteTrackingGeraete { get; } = [];

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
            return Task.FromResult<TrackingGeraetStandortAktuell?>(null);
        }

        public Task<TrackingGeraetStandortHistorienpunkt?> GetLetztenTrackingGeraetHistorienpunktAsync(
            Guid trackingGeraetId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TrackingGeraetStandortHistorienpunkt?>(null);
        }

        public Task<IReadOnlyList<TrackingGeraetStandortHistorienpunkt>> GetTrackingGeraetHistorienpunkteAsync(
            Guid trackingGeraetId,
            Guid? routeSessionId,
            int maxCount,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<TrackingGeraetStandortHistorienpunkt> result = [];
            return Task.FromResult(result);
        }

        public Task AddTrackingGeraetAsync(
            FahrzeugTrackingGeraet trackingGeraet,
            CancellationToken cancellationToken = default)
        {
            HinzugefuegteTrackingGeraete.Add(trackingGeraet);
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
            return Task.CompletedTask;
        }

        public Task AddTrackingGeraetHistorienpunktAsync(
            TrackingGeraetStandortHistorienpunkt historienpunkt,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAufrufe++;
            return Task.CompletedTask;
        }
    }
}