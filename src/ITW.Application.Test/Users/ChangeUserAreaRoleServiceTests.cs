using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Users.ChangeAreaRole;
using ITW.Domain.Organisation.Entities;
using ITW.Domain.Organisation.Enums;
using Xunit;

namespace ITW.Application.Test.Users;

public sealed class ChangeUserAreaRoleServiceTests
{
    private static readonly DateTimeOffset _fixedNow = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_GueltigeRollenAenderung_AendertRolleErfolgreich()
    {
        var zuordnung = new BenutzerBereichszuordnung(
            Guid.NewGuid(),
            "user-1",
            Organisationsbereich.Intensivtransport,
            Bereichsrolle.ItwMitarbeiter,
            Fuehrungsverantwortung.Keine,
            true,
            _fixedNow);

        var repository = new FakeBenutzerBereichszuordnungRepository(zuordnung);
        var service = new ChangeUserAreaRoleService(repository);

        var command = new ChangeUserAreaRoleCommand(
            "user-1",
            BereichsrolleCode.Wachleiter,
            FuehrungsverantwortungCode.OperativeLeitung);

        var result = await service.ExecuteAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(Bereichsrolle.Wachleiter, zuordnung.Rolle);
        Assert.Equal(1, repository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ExecuteAsync_KeineBestehendePrimaereZuordnung_GibtFehlerZurueck()
    {
        var repository = new FakeBenutzerBereichszuordnungRepository(bestehendeZuordnung: null);
        var service = new ChangeUserAreaRoleService(repository);

        var command = new ChangeUserAreaRoleCommand(
            "user-1",
            BereichsrolleCode.Wachleiter,
            FuehrungsverantwortungCode.OperativeLeitung);

        var result = await service.ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal(0, repository.SaveChangesAufrufe);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_LeereUserId_GibtValidierungsfehlerZurueck(string userId)
    {
        var repository = new FakeBenutzerBereichszuordnungRepository();
        var service = new ChangeUserAreaRoleService(repository);

        var command = new ChangeUserAreaRoleCommand(
            userId,
            BereichsrolleCode.Mitarbeiter,
            FuehrungsverantwortungCode.Keine);

        var result = await service.ExecuteAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Contains("UserId", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, repository.SaveChangesAufrufe);
    }

    private sealed class FakeBenutzerBereichszuordnungRepository : IBenutzerBereichszuordnungRepository
    {
        private readonly BenutzerBereichszuordnung? _bestehendeZuordnung;

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
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAufrufe++;
            return Task.CompletedTask;
        }
    }
}
