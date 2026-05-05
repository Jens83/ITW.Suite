using ITW.Application.Abstractions.Identity;
using ITW.Application.Users.LockUser;
using Xunit;

namespace ITW.Application.Test.Users;

public sealed class LockUserServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ErfolgreichesSperren_GibtErfolgZurueck()
    {
        var repository = new FakeBenutzerkontoRepository(
            sperrenErgebnis: UpdateBenutzerkontoStatusRepositoryResult.Erfolg());
        var service = new LockUserService(repository);

        var result = await service.ExecuteAsync(new LockUserCommand("user-1"));

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_RepositoryFehler_GibtFehlerZurueck()
    {
        var repository = new FakeBenutzerkontoRepository(
            sperrenErgebnis: UpdateBenutzerkontoStatusRepositoryResult.Fehler("Benutzer nicht gefunden."));
        var service = new LockUserService(repository);

        var result = await service.ExecuteAsync(new LockUserCommand("user-1"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Benutzer nicht gefunden.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_LeereUserId_GibtValidierungsfehlerZurueck(string userId)
    {
        var repository = new FakeBenutzerkontoRepository();
        var service = new LockUserService(repository);

        var result = await service.ExecuteAsync(new LockUserCommand(userId));

        Assert.False(result.IsSuccess);
        Assert.Contains("UserId", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, repository.SperrenAufrufe);
    }

    private sealed class FakeBenutzerkontoRepository : IBenutzerkontoRepository
    {
        private readonly UpdateBenutzerkontoStatusRepositoryResult _sperrenErgebnis;

        public int SperrenAufrufe { get; private set; }

        public FakeBenutzerkontoRepository(
            UpdateBenutzerkontoStatusRepositoryResult? sperrenErgebnis = null)
        {
            _sperrenErgebnis = sperrenErgebnis
                ?? UpdateBenutzerkontoStatusRepositoryResult.Erfolg();
        }

        public Task<UpdateBenutzerkontoStatusRepositoryResult> SperrenAsync(
            string userId, CancellationToken cancellationToken = default)
        {
            SperrenAufrufe++;
            return Task.FromResult(_sperrenErgebnis);
        }

        public Task<CreateBenutzerkontoRepositoryResult> CreateAsync(
            string benutzername, string email, string passwort,
            CancellationToken cancellationToken = default)
            => Task.FromResult(CreateBenutzerkontoRepositoryResult.Erfolg(
                new BenutzerkontoDto("user-1", benutzername, email, false)));

        public Task<IReadOnlyList<BenutzerkontoDto>> GetByIdsAsync(
            IEnumerable<string> userIds, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BenutzerkontoDto>>(Array.Empty<BenutzerkontoDto>());

        public Task<IReadOnlyList<BenutzerkontoDto>> GetNichtZugeordneteBenutzerkontenAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BenutzerkontoDto>>(Array.Empty<BenutzerkontoDto>());

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
