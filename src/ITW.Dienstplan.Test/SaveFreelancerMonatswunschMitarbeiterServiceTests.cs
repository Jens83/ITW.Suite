using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Personnel.ProfileQueries;
using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Wunschphase;
using ITW.Dienstplan.Test.Helpers;
using ITW.Dienstplan.Domain.Entities;
using ITW.Domain.Organisation.Entities;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Enums;
using ITW.Domain.Personnel.Qualifications;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Write;
using Xunit;

namespace ITW.Web.Test;

public sealed class SaveFreelancerMonatswunschMitarbeiterServiceTests
{
    [Fact]
    public async Task ExecuteAsync_GibtFehlerWennBenutzerKeinFreelancerIst()
    {
        // Arrange
        var userId = "mitarbeiter-1";
        var periodeId = Guid.NewGuid();

        var leseMitarbeiterService = ErzeugeReadItwMitarbeiterprofileService(
            userId,
            MitarbeiterBeschaeftigungsart.Festangestellt);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            Periode = ErzeugePeriode(periodeId, 2026, 5, wunschphaseIstOffen: true)
        };

        var freelancerMonatswunschRepository = new FakeFreelancerMonatswunschRepository();

        var saveFreelancerMonatswunschService = new SaveFreelancerMonatswunschService(
            dienstplanPeriodeRepository,
            freelancerMonatswunschRepository);

        var service = new SaveFreelancerMonatswunschMitarbeiterService(
            leseMitarbeiterService,
            saveFreelancerMonatswunschService);

        // Act
        var result = await service.ExecuteAsync(
            new SaveFreelancerMonatswunschMitarbeiterCommand(
                periodeId,
                userId,
                2),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Die gewünschte Monatsanzahl kann nur für Freelancer gespeichert werden.",
            result.ErrorMessage);

        Assert.Empty(freelancerMonatswunschRepository.Eintraege);
        Assert.Equal(0, freelancerMonatswunschRepository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ExecuteAsync_SpeichertMonatswunschWennBenutzerFreelancerIst()
    {
        // Arrange
        var userId = "freelancer-1";
        var periodeId = Guid.NewGuid();

        var leseMitarbeiterService = ErzeugeReadItwMitarbeiterprofileService(
            userId,
            MitarbeiterBeschaeftigungsart.Freelancer);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            Periode = ErzeugePeriode(periodeId, 2026, 5, wunschphaseIstOffen: true)
        };

        var freelancerMonatswunschRepository = new FakeFreelancerMonatswunschRepository();

        var saveFreelancerMonatswunschService = new SaveFreelancerMonatswunschService(
            dienstplanPeriodeRepository,
            freelancerMonatswunschRepository);

        var service = new SaveFreelancerMonatswunschMitarbeiterService(
            leseMitarbeiterService,
            saveFreelancerMonatswunschService);

        // Act
        var result = await service.ExecuteAsync(
            new SaveFreelancerMonatswunschMitarbeiterCommand(
                periodeId,
                userId,
                3),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var eintrag = Assert.Single(freelancerMonatswunschRepository.Eintraege);
        Assert.Equal(periodeId, eintrag.DienstplanPeriodeId);
        Assert.Equal(userId, eintrag.UserId);
        Assert.Equal(3, eintrag.GewuenschteDienste);
        Assert.Equal(1, freelancerMonatswunschRepository.SaveChangesAufrufe);
    }

    private static ReadItwMitarbeiterprofileService ErzeugeReadItwMitarbeiterprofileService(
        string userId,
        MitarbeiterBeschaeftigungsart beschaeftigungsart)
    {
        var bereichszuordnungRepository = new FakeBenutzerBereichszuordnungRepository
        {
            Zuordnungen =
            [
                new BenutzerBereichszuordnung(
                    Guid.NewGuid(),
                    userId,
                    Organisationsbereich.Intensivtransport,
                    Bereichsrolle.ItwMitarbeiter,
                    Fuehrungsverantwortung.Keine,
                    true,
                    DateTimeOffset.UtcNow)
            ]
        };

        var benutzerkontoRepository = new FakeBenutzerkontoRepository
        {
            Konten =
            [
                new BenutzerkontoDto(
                    userId,
                    userId,
                    $"{userId}@example.invalid",
                    false)
            ]
        };

        var allgemeinesProfil = new AllgemeinesMitarbeiterprofil(
            Guid.NewGuid(),
            userId,
            DateTimeOffset.UtcNow);

        allgemeinesProfil.AktualisiereStammdaten(
            "Max",
            "Mustermann",
            beschaeftigungsart,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);

        var itwProfil = new ItwMitarbeiterprofil(
            Guid.NewGuid(),
            userId,
            DateTimeOffset.UtcNow);

        return new ReadItwMitarbeiterprofileService(
            bereichszuordnungRepository,
            benutzerkontoRepository,
            new FakeItwMitarbeiterprofilRepository
            {
                Profile = [itwProfil]
            },
            new FakeAllgemeinesMitarbeiterprofilRepository
            {
                Profile = [allgemeinesProfil]
            },
            FakeLogger<ReadItwMitarbeiterprofileService>.Instance);
    }

    private static DienstplanPeriode ErzeugePeriode(
        Guid periodeId,
        int jahr,
        int monat,
        bool wunschphaseIstOffen)
    {
        return new DienstplanPeriode(
            periodeId,
            jahr,
            monat,
            $"Periode {monat:D2}/{jahr}",
            wunschphaseIstOffen,
            "system",
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeDienstplanPeriodeRepository : IDienstplanPeriodeRepository
    {
        public DienstplanPeriode? Periode { get; set; }

        public Task<bool> ExistiertAsync(
            int jahr,
            int monat,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddAsync(
            DienstplanPeriode periode,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<DienstplanPeriode?> GetByIdAsync(
            Guid periodeId,
            CancellationToken cancellationToken = default)
        {
            if (Periode is not null && Periode.Id == periodeId)
            {
                return Task.FromResult<DienstplanPeriode?>(Periode);
            }

            return Task.FromResult<DienstplanPeriode?>(null);
        }

        public Task<DienstplanPeriode?> GetAktuelleOffeneAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<DienstplanPeriode?>(null);

        public Task<int> CountOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Periode is not null && Periode.WunschphaseIstOffen ? 1 : 0);

        public Task<int> CountOffeneFuerBenutzerOhneWuenscheAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<IReadOnlyList<DienstplanPeriode>> GetOffeneAsync(
            CancellationToken cancellationToken = default)
        {
            if (Periode is not null && Periode.WunschphaseIstOffen)
            {
                return Task.FromResult<IReadOnlyList<DienstplanPeriode>>([Periode]);
            }

            return Task.FromResult<IReadOnlyList<DienstplanPeriode>>(Array.Empty<DienstplanPeriode>());
        }

        public Task<IReadOnlyList<DienstplanPeriode>> GetAlleAsync(
            CancellationToken cancellationToken = default)
        {
            if (Periode is not null)
            {
                return Task.FromResult<IReadOnlyList<DienstplanPeriode>>([Periode]);
            }

            return Task.FromResult<IReadOnlyList<DienstplanPeriode>>(Array.Empty<DienstplanPeriode>());
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeFreelancerMonatswunschRepository : IFreelancerMonatswunschRepository
    {
        public List<FreelancerMonatswunsch> Eintraege { get; } = [];
        public int SaveChangesAufrufe { get; private set; }

        public Task<FreelancerMonatswunsch?> GetAsync(
            Guid dienstplanPeriodeId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = Eintraege.FirstOrDefault(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<FreelancerMonatswunsch>> GetAlleFuerPeriodeAsync(
            Guid dienstplanPeriodeId,
            CancellationToken cancellationToken = default)
        {
            var result = Eintraege
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
                .ToArray();

            return Task.FromResult<IReadOnlyList<FreelancerMonatswunsch>>(result);
        }

        public Task AddAsync(
            FreelancerMonatswunsch eintrag,
            CancellationToken cancellationToken = default)
        {
            Eintraege.Add(eintrag);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAufrufe++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBenutzerBereichszuordnungRepository : IBenutzerBereichszuordnungRepository
    {
        public IReadOnlyList<BenutzerBereichszuordnung> Zuordnungen { get; set; }
            = Array.Empty<BenutzerBereichszuordnung>();

        public Task<BenutzerBereichszuordnung?> GetAktivePrimaereZuordnungAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = Zuordnungen.FirstOrDefault(x =>
                x.IsActive &&
                x.IsPrimary &&
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<BenutzerBereichszuordnung>> GetAktivePrimaereZuordnungenByBereichAsync(
            Organisationsbereich bereich,
            CancellationToken cancellationToken = default)
        {
            var result = Zuordnungen
                .Where(x => x.IsActive && x.IsPrimary && x.Bereich == bereich)
                .ToArray();

            return Task.FromResult<IReadOnlyList<BenutzerBereichszuordnung>>(result);
        }

        public Task AddAsync(
            BenutzerBereichszuordnung zuordnung,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeBenutzerkontoRepository : IBenutzerkontoRepository
    {
        public IReadOnlyList<BenutzerkontoDto> Konten { get; set; } = Array.Empty<BenutzerkontoDto>();

        public Task<IReadOnlyList<BenutzerkontoDto>> GetByIdsAsync(
            IEnumerable<string> userIds,
            CancellationToken cancellationToken = default)
        {
            var idSet = userIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = Konten
                .Where(x => idSet.Contains(x.UserId))
                .ToArray();

            return Task.FromResult<IReadOnlyList<BenutzerkontoDto>>(result);
        }

        public Task<IReadOnlyList<BenutzerkontoDto>> GetNichtZugeordneteBenutzerkontenAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BenutzerkontoDto>>(Array.Empty<BenutzerkontoDto>());

        public Task<CreateBenutzerkontoRepositoryResult> CreateAsync(
            string benutzername,
            string email,
            string passwort,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<UpdateBenutzerkontoStatusRepositoryResult> SperrenAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<UpdateBenutzerkontoStatusRepositoryResult> AktivierenAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SetTemporaeresPasswortRepositoryResult> SetzeTemporaeresPasswortAsync(
            string userId,
            string temporaeresPasswort,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task EntfernePasswortwechselPflichtAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SynchronisiereNamensClaimsAsync(
            string userId,
            string? vorname,
            string? nachname,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeItwMitarbeiterprofilRepository : IItwMitarbeiterprofilRepository
    {
        public IReadOnlyList<ItwMitarbeiterprofil> Profile { get; set; } = Array.Empty<ItwMitarbeiterprofil>();

        public Task EnsureStandardqualifikationenAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<ItwQualifikation>> GetAktiveQualifikationenAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ItwQualifikation>>(Array.Empty<ItwQualifikation>());

        public Task<IReadOnlyList<ItwMitarbeiterprofil>> GetByUserIdsAsync(
            IEnumerable<string> userIds,
            CancellationToken cancellationToken = default)
        {
            var idSet = userIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = Profile
                .Where(x => idSet.Contains(x.UserId))
                .ToArray();

            return Task.FromResult<IReadOnlyList<ItwMitarbeiterprofil>>(result);
        }

        public Task<ItwMitarbeiterprofil?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = Profile.FirstOrDefault(x =>
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task UpsertQualifikationenAsync(
            string userId,
            Guid hauptqualifikationId,
            IReadOnlyCollection<Guid> zusatzqualifikationIds,
            DateTimeOffset aktualisiertAm,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeAllgemeinesMitarbeiterprofilRepository : IAllgemeinesMitarbeiterprofilRepository
    {
        public IReadOnlyList<AllgemeinesMitarbeiterprofil> Profile { get; set; }
            = Array.Empty<AllgemeinesMitarbeiterprofil>();

        public Task<AllgemeinesMitarbeiterprofil?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = Profile.FirstOrDefault(x =>
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<AllgemeinesMitarbeiterprofil>> GetByUserIdsAsync(
            IReadOnlyCollection<string> userIds,
            CancellationToken cancellationToken = default)
        {
            var idSet = userIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = Profile
                .Where(x => idSet.Contains(x.UserId))
                .ToArray();

            return Task.FromResult<IReadOnlyList<AllgemeinesMitarbeiterprofil>>(result);
        }

        public Task UpsertAsync(
            string userId,
            string vorname,
            string nachname,
            MitarbeiterBeschaeftigungsart beschaeftigungsart,
            string? telefonnummer,
            string? strasse,
            string? hausnummer,
            string? postleitzahl,
            string? ort,
            DateTimeOffset aktualisiertAm,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}