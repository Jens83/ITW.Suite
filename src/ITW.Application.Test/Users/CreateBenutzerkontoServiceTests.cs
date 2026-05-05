using ITW.Application.Abstractions.Identity;
using ITW.Application.Users.CreateUser;
using Xunit;

namespace ITW.Application.Test.Users;

public sealed class CreateBenutzerkontoServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ErfolgreicheErstellung_GibtErfolgZurueck()
    {
        var dto = new BenutzerkontoDto("user-1", "maxmuster", "max@example.invalid", false);
        var repository = new FakeBenutzerkontoRepository(
            createResult: CreateBenutzerkontoRepositoryResult.Erfolg(dto));

        var service = new CreateBenutzerkontoService(repository);

        var result = await service.ExecuteAsync(
            new CreateBenutzerkontoCommand("maxmuster", "max@example.invalid", "Passwort123!"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Benutzerkonto);
        Assert.Null(result.ErrorMessage);
        Assert.Equal("user-1", result.Benutzerkonto.UserId);
    }

    [Fact]
    public async Task ExecuteAsync_RepositoryFehler_GibtFehlerZurueck()
    {
        var repository = new FakeBenutzerkontoRepository(
            createResult: CreateBenutzerkontoRepositoryResult.Fehler("Benutzername bereits vergeben."));

        var service = new CreateBenutzerkontoService(repository);

        var result = await service.ExecuteAsync(
            new CreateBenutzerkontoCommand("maxmuster", "max@example.invalid", "Passwort123!"));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Benutzerkonto);
        Assert.Equal("Benutzername bereits vergeben.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("", "max@example.invalid", "Passwort123!")]
    [InlineData("   ", "max@example.invalid", "Passwort123!")]
    public async Task ExecuteAsync_LeererBenutzername_GibtValidierungsfehlerZurueck(
        string benutzername, string email, string passwort)
    {
        var repository = new FakeBenutzerkontoRepository();
        var service = new CreateBenutzerkontoService(repository);

        var result = await service.ExecuteAsync(
            new CreateBenutzerkontoCommand(benutzername, email, passwort));

        Assert.False(result.IsSuccess);
        Assert.Contains("Benutzername", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, repository.CreateAufrufe);
    }

    [Theory]
    [InlineData("maxmuster", "", "Passwort123!")]
    [InlineData("maxmuster", "   ", "Passwort123!")]
    public async Task ExecuteAsync_LeereEmail_GibtValidierungsfehlerZurueck(
        string benutzername, string email, string passwort)
    {
        var repository = new FakeBenutzerkontoRepository();
        var service = new CreateBenutzerkontoService(repository);

        var result = await service.ExecuteAsync(
            new CreateBenutzerkontoCommand(benutzername, email, passwort));

        Assert.False(result.IsSuccess);
        Assert.Contains("E-Mail", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, repository.CreateAufrufe);
    }

    [Theory]
    [InlineData("maxmuster", "max@example.invalid", "")]
    [InlineData("maxmuster", "max@example.invalid", "   ")]
    public async Task ExecuteAsync_LeeresPasword_GibtValidierungsfehlerZurueck(
        string benutzername, string email, string passwort)
    {
        var repository = new FakeBenutzerkontoRepository();
        var service = new CreateBenutzerkontoService(repository);

        var result = await service.ExecuteAsync(
            new CreateBenutzerkontoCommand(benutzername, email, passwort));

        Assert.False(result.IsSuccess);
        Assert.Contains("Passwort", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, repository.CreateAufrufe);
    }

    private sealed class FakeBenutzerkontoRepository : IBenutzerkontoRepository
    {
        private readonly CreateBenutzerkontoRepositoryResult _createResult;

        public int CreateAufrufe { get; private set; }

        public FakeBenutzerkontoRepository(
            CreateBenutzerkontoRepositoryResult? createResult = null)
        {
            _createResult = createResult
                ?? CreateBenutzerkontoRepositoryResult.Erfolg(
                    new BenutzerkontoDto("user-1", "test", "test@example.invalid", false));
        }

        public Task<CreateBenutzerkontoRepositoryResult> CreateAsync(
            string benutzername, string email, string passwort,
            CancellationToken cancellationToken = default)
        {
            CreateAufrufe++;
            return Task.FromResult(_createResult);
        }

        public Task<IReadOnlyList<BenutzerkontoDto>> GetByIdsAsync(
            IEnumerable<string> userIds, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BenutzerkontoDto>>(Array.Empty<BenutzerkontoDto>());

        public Task<IReadOnlyList<BenutzerkontoDto>> GetNichtZugeordneteBenutzerkontenAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BenutzerkontoDto>>(Array.Empty<BenutzerkontoDto>());

        public Task<UpdateBenutzerkontoStatusRepositoryResult> SperrenAsync(
            string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(UpdateBenutzerkontoStatusRepositoryResult.Erfolg());

        public Task<UpdateBenutzerkontoStatusRepositoryResult> AktivierenAsync(
            string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(UpdateBenutzerkontoStatusRepositoryResult.Erfolg());

        public Task<SetTemporaeresPasswortRepositoryResult> SetzeTemporaeresPasswortAsync(
            string userId, string temporaeresPasswort,
            CancellationToken cancellationToken = default)
            => Task.FromResult(SetTemporaeresPasswortRepositoryResult.Erfolg());

        public Task EntfernePasswortwechselPflichtAsync(
            string userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SynchronisiereNamensClaimsAsync(
            string userId, string? vorname, string? nachname,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
