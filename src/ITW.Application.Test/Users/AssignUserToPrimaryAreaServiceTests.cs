using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Users.AssignArea;
using ITW.Domain.Organisation.Entities;
using ITW.Domain.Organisation.Enums;
using Xunit;

namespace ITW.Application.Test.Users;

public sealed class AssignUserToPrimaryAreaServiceTests
{
    private static readonly DateTimeOffset _fixedNow = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_KeineBestehendeZuordnung_Legt_NeueZuordnungAn()
    {
        var repository = new FakeBenutzerBereichszuordnungRepository();
        var service = new AssignUserToPrimaryAreaService(repository, new FakeDateTimeProvider(_fixedNow));

        var command = new AssignUserToPrimaryAreaCommand(
            "user-1",
            OrganisationsbereichCode.Intensivtransport,
            BereichsrolleCode.Mitarbeiter,
            FuehrungsverantwortungCode.Keine);

        var result = await service.ExecuteAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.BereichszuordnungId);
        Assert.Single(repository.HinzugefuegteZuordnungen);
        Assert.Equal(1, repository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ExecuteAsync_IdentischeBestehendeZuordnung_GibtErfolgOhneAenderungZurueck()
    {
        var bestehendeId = Guid.NewGuid();
        var bestehendeZuordnung = new BenutzerBereichszuordnung(
            bestehendeId,
            "user-1",
            Organisationsbereich.Intensivtransport,
            Bereichsrolle.ItwMitarbeiter,
            Fuehrungsverantwortung.Keine,
            true,
            _fixedNow);

        var repository = new FakeBenutzerBereichszuordnungRepository(bestehendeZuordnung);
        var service = new AssignUserToPrimaryAreaService(repository, new FakeDateTimeProvider(_fixedNow));

        var command = new AssignUserToPrimaryAreaCommand(
            "user-1",
            OrganisationsbereichCode.Intensivtransport,
            BereichsrolleCode.Mitarbeiter,
            FuehrungsverantwortungCode.Keine);

        var result = await service.ExecuteAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(bestehendeId, result.BereichszuordnungId);
        Assert.Empty(repository.HinzugefuegteZuordnungen);
        Assert.Equal(0, repository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ExecuteAsync_BestehendeZuordnungOhneErsetzFlag_GibtFehlerZurueck()
    {
        var bestehendeZuordnung = new BenutzerBereichszuordnung(
            Guid.NewGuid(),
            "user-1",
            Organisationsbereich.Intensivtransport,
            Bereichsrolle.ItwMitarbeiter,
            Fuehrungsverantwortung.Keine,
            true,
            _fixedNow);

        var repository = new FakeBenutzerBereichszuordnungRepository(bestehendeZuordnung);
        var service = new AssignUserToPrimaryAreaService(repository, new FakeDateTimeProvider(_fixedNow));

        var command = new AssignUserToPrimaryAreaCommand(
            "user-1",
            OrganisationsbereichCode.Intensivtransport,
            BereichsrolleCode.Wachleiter,
            FuehrungsverantwortungCode.OperativeLeitung,
            BestehendePrimaereZuordnungErsetzen: false);

        var result = await service.ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Empty(repository.HinzugefuegteZuordnungen);
    }

    [Fact]
    public async Task ExecuteAsync_BestehendeZuordnungMitErsetzFlag_ErsetztUndLegtNeueAn()
    {
        var bestehendeZuordnung = new BenutzerBereichszuordnung(
            Guid.NewGuid(),
            "user-1",
            Organisationsbereich.Intensivtransport,
            Bereichsrolle.ItwMitarbeiter,
            Fuehrungsverantwortung.Keine,
            true,
            _fixedNow);

        var repository = new FakeBenutzerBereichszuordnungRepository(bestehendeZuordnung);
        var service = new AssignUserToPrimaryAreaService(repository, new FakeDateTimeProvider(_fixedNow));

        var command = new AssignUserToPrimaryAreaCommand(
            "user-1",
            OrganisationsbereichCode.Intensivtransport,
            BereichsrolleCode.Wachleiter,
            FuehrungsverantwortungCode.OperativeLeitung,
            BestehendePrimaereZuordnungErsetzen: true);

        var result = await service.ExecuteAsync(command);

        Assert.True(result.IsSuccess);
        Assert.False(bestehendeZuordnung.IsActive);
        Assert.Single(repository.HinzugefuegteZuordnungen);
        Assert.Equal(1, repository.SaveChangesAufrufe);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_LeereUserId_GibtValidierungsfehlerZurueck(string userId)
    {
        var repository = new FakeBenutzerBereichszuordnungRepository();
        var service = new AssignUserToPrimaryAreaService(repository, new FakeDateTimeProvider(_fixedNow));

        var command = new AssignUserToPrimaryAreaCommand(
            userId,
            OrganisationsbereichCode.Intensivtransport,
            BereichsrolleCode.Mitarbeiter,
            FuehrungsverantwortungCode.Keine);

        var result = await service.ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Contains("UserId", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(repository.HinzugefuegteZuordnungen);
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public FakeDateTimeProvider(DateTimeOffset utcNow) => UtcNow = utcNow;
        public DateTimeOffset UtcNow { get; }
    }

    private sealed class FakeBenutzerBereichszuordnungRepository : IBenutzerBereichszuordnungRepository
    {
        private readonly BenutzerBereichszuordnung? _bestehendeZuordnung;

        public List<BenutzerBereichszuordnung> HinzugefuegteZuordnungen { get; } = [];
        public int SaveChangesAufrufe { get; private set; }

        public FakeBenutzerBereichszuordnungRepository(
            BenutzerBereichszuordnung? bestehendeZuordnung = null)
        {
            _bestehendeZuordnung = bestehendeZuordnung;
        }

        public Task<BenutzerBereichszuordnung?> GetAktivePrimaereZuordnungAsync(
            string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_bestehendeZuordnung);

        public Task<IReadOnlyList<BenutzerBereichszuordnung>> GetAktivePrimaereZuordnungenByBereichAsync(
            Organisationsbereich bereich, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BenutzerBereichszuordnung>>(Array.Empty<BenutzerBereichszuordnung>());

        public Task AddAsync(
            BenutzerBereichszuordnung zuordnung, CancellationToken cancellationToken = default)
        {
            HinzugefuegteZuordnungen.Add(zuordnung);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAufrufe++;
            return Task.CompletedTask;
        }
    }
}
