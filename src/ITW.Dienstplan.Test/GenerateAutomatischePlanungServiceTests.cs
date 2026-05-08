using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Domain.Personnel.Enums;
using ITW.Domain.Personnel.Qualifications;
using Xunit;

namespace ITW.Dienstplan.Test;

public sealed class GenerateAutomatischePlanungServiceTests
{
    [Fact]
    public async Task ExecuteAsync_SchreibtBestaetigungsLernereignisseBeimBewusstenAutoplanSpeichern()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var dienstDatum = new DateOnly(2026, 4, 1);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 4)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter("arzt.freelancer", "Dr. Auto", MitarbeiterBeschaeftigungsart.Freelancer, ItwQualifikationsCodes.Arzt, "Arzt"),
                ErzeugeMitarbeiter("nfs.eins", "NFS Eins", MitarbeiterBeschaeftigungsart.Freelancer, ItwQualifikationsCodes.Notfallsanitaeter, "Notfallsanitäter"),
                ErzeugeMitarbeiter("nfs.zwei", "NFS Zwei", MitarbeiterBeschaeftigungsart.Freelancer, ItwQualifikationsCodes.Notfallsanitaeter, "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, "arzt.freelancer", dienstDatum, DienstwunschTyp.Wunsch),
                ErzeugeWunsch(periodeId, "nfs.eins", dienstDatum, DienstwunschTyp.Wunsch),
                ErzeugeWunsch(periodeId, "nfs.zwei", dienstDatum, DienstwunschTyp.Wunsch)
            ]
        };

        var freelancerMonatswunschRepository = new FakeFreelancerMonatswunschRepository
        {
            Eintraege =
            [
                ErzeugeFreelancerMonatswunsch(periodeId, "arzt.freelancer", 1),
                ErzeugeFreelancerMonatswunsch(periodeId, "nfs.eins", 1),
                ErzeugeFreelancerMonatswunsch(periodeId, "nfs.zwei", 1)
            ]
        };

        var geplanterDienstTagRepository = new FakeGeplanterDienstTagRepository();
        var ausfallRepository = new FakeDienstbesetzungsAusfallRepository();
        var urlaubsRepository = new FakeDienstplanUrlaubszeitraumRepository();
        var lernereignisRepository = new FakeAutoplanLernereignisRepository();

        var service = new GenerateAutomatischePlanungService(
            dienstplanPeriodeRepository,
            mitarbeiterRepository,
            dienstwunschRepository,
            freelancerMonatswunschRepository,
            geplanterDienstTagRepository,
            ausfallRepository,
            urlaubsRepository,
            lernereignisRepository);

        // Act
        var result = await service.ExecuteAsync(
            new GenerateAutomatischePlanungCommand(
                periodeId,
                "wachleiter-1",
                AutomatischePlanungAusfuehrungsmodus.NurOffeneSlotsFuellen));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var geplanterTag = Assert.Single(geplanterDienstTagRepository.Eintraege);
        Assert.Equal(dienstDatum, geplanterTag.DienstDatum);
        Assert.Equal("arzt.freelancer", geplanterTag.ArztUserId);
        Assert.Equal("nfs.eins", geplanterTag.Notfallsanitaeter1UserId);
        Assert.Equal("nfs.zwei", geplanterTag.Notfallsanitaeter2UserId);

        var bestaetigungen = lernereignisRepository.AddedLernereignisse
            .Where(x => x.EreignisTypCode == AutoplanLernereignisTypCode.AutoplanVorschlagBestaetigt)
            .ToArray();

        Assert.Equal(3, bestaetigungen.Length);
        Assert.Contains(bestaetigungen, x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Arzt && x.NeueUserId == "arzt.freelancer");
        Assert.Contains(bestaetigungen, x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Notfallsanitaeter1 && x.NeueUserId == "nfs.eins");
        Assert.Contains(bestaetigungen, x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Notfallsanitaeter2 && x.NeueUserId == "nfs.zwei");
        Assert.Equal(1, lernereignisRepository.SaveChangesCount);
    }

    private static DienstplanPeriode ErzeugePeriode(Guid id, int jahr, int monat)
    {
        return new DienstplanPeriode(
            id,
            jahr,
            monat,
            $"{monat:00}/{jahr}",
            wunschphaseIstOffen: false,
            erstelltVonUserId: "wachleiter-1",
            erstelltAm: new DateTimeOffset(jahr, monat, 1, 8, 0, 0, TimeSpan.Zero));
    }

    private static DienstplanMitarbeiterPlanungsstammdaten ErzeugeMitarbeiter(
        string userId,
        string anzeigeName,
        MitarbeiterBeschaeftigungsart beschaeftigungsart,
        string qualifikationCode,
        string qualifikationBezeichnung)
    {
        return new DienstplanMitarbeiterPlanungsstammdaten
        {
            UserId = userId,
            AnzeigeName = anzeigeName,
            Beschaeftigungsart = beschaeftigungsart,
            HauptqualifikationCode = qualifikationCode,
            HauptqualifikationBezeichnung = qualifikationBezeichnung,
            IstGesperrt = false,
            HatStammdatenprofil = true,
            HatItwProfil = true
        };
    }

    private static Dienstwunsch ErzeugeWunsch(
        Guid periodeId,
        string userId,
        DateOnly datum,
        DienstwunschTyp typ)
    {
        return new Dienstwunsch(
            Guid.NewGuid(),
            periodeId,
            userId,
            datum,
            typ,
            new DateTimeOffset(datum.Year, datum.Month, datum.Day, 8, 0, 0, TimeSpan.Zero));
    }

    private static FreelancerMonatswunsch ErzeugeFreelancerMonatswunsch(
        Guid periodeId,
        string userId,
        int gewuenschteDienste)
    {
        return new FreelancerMonatswunsch(
            Guid.NewGuid(),
            periodeId,
            userId,
            gewuenschteDienste,
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeDienstplanPeriodeRepository : IDienstplanPeriodeRepository
    {
        public DienstplanPeriode? PeriodeById { get; set; }

        public Task<bool> ExistiertAsync(int jahr, int monat, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddAsync(DienstplanPeriode periode, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<DienstplanPeriode?> GetByIdAsync(Guid periodeId, CancellationToken cancellationToken = default)
            => Task.FromResult(PeriodeById);

        public Task<DienstplanPeriode?> GetAktuelleOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<DienstplanPeriode?>(null);

        public Task<int> CountOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> CountOffeneFuerBenutzerOhneWuenscheAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<IReadOnlyList<DienstplanPeriode>> GetOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DienstplanPeriode>>(Array.Empty<DienstplanPeriode>());

        public Task<IReadOnlyList<DienstplanPeriode>> GetAlleAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DienstplanPeriode>>(PeriodeById is null ? Array.Empty<DienstplanPeriode>() : [PeriodeById]);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDienstplanMitarbeiterPlanungsRepository : IDienstplanMitarbeiterPlanungsRepository
    {
        public IReadOnlyList<DienstplanMitarbeiterPlanungsstammdaten> Mitarbeiter { get; set; }
            = Array.Empty<DienstplanMitarbeiterPlanungsstammdaten>();

        public Task<IReadOnlyList<DienstplanMitarbeiterPlanungsstammdaten>> GetAktivePlanungsmitarbeiterAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(Mitarbeiter);
    }

    private sealed class FakeDienstwunschRepository : IDienstwunschRepository
    {
        public List<Dienstwunsch> Wuensche { get; set; } = new();

        public Task<Dienstwunsch?> GetAsync(Guid dienstplanPeriodeId, string userId, DateOnly wunschDatum, CancellationToken cancellationToken = default)
            => Task.FromResult(Wuensche.FirstOrDefault(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase) &&
                x.WunschDatum == wunschDatum));

        public Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerBenutzerAsync(Guid dienstplanPeriodeId, string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Dienstwunsch>>(Wuensche
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId && string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase))
                .ToArray());

        public Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerPeriodeAsync(Guid dienstplanPeriodeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Dienstwunsch>>(Wuensche
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
                .ToArray());

        public Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerTagAsync(Guid dienstplanPeriodeId, DateOnly wunschDatum, DienstwunschTyp? wunschTyp = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Dienstwunsch>>(Wuensche
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId && x.WunschDatum == wunschDatum && (!wunschTyp.HasValue || x.WunschTyp == wunschTyp.Value))
                .ToArray());

        public Task AddAsync(Dienstwunsch dienstwunsch, CancellationToken cancellationToken = default)
        {
            Wuensche.Add(dienstwunsch);
            return Task.CompletedTask;
        }

        public void Remove(Dienstwunsch dienstwunsch)
        {
            Wuensche.Remove(dienstwunsch);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeFreelancerMonatswunschRepository : IFreelancerMonatswunschRepository
    {
        public List<FreelancerMonatswunsch> Eintraege { get; set; } = new();

        public Task<FreelancerMonatswunsch?> GetAsync(Guid dienstplanPeriodeId, string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Eintraege.FirstOrDefault(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyList<FreelancerMonatswunsch>> GetAlleFuerPeriodeAsync(Guid dienstplanPeriodeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<FreelancerMonatswunsch>>(Eintraege
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
                .ToArray());

        public Task AddAsync(FreelancerMonatswunsch eintrag, CancellationToken cancellationToken = default)
        {
            Eintraege.Add(eintrag);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeGeplanterDienstTagRepository : IGeplanterDienstTagRepository
    {
        public List<GeplanterDienstTag> Eintraege { get; } = new();

        public Task<GeplanterDienstTag?> GetAsync(Guid dienstplanPeriodeId, DateOnly dienstDatum, CancellationToken cancellationToken = default)
            => Task.FromResult(Eintraege.FirstOrDefault(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                x.DienstDatum == dienstDatum));

        public Task<IReadOnlyList<GeplanterDienstTag>> GetAlleFuerPeriodeAsync(Guid dienstplanPeriodeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<GeplanterDienstTag>>(Eintraege
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
                .ToArray());

        public Task AddAsync(GeplanterDienstTag geplanterDienstTag, CancellationToken cancellationToken = default)
        {
            Eintraege.Add(geplanterDienstTag);
            return Task.CompletedTask;
        }

        public void Remove(GeplanterDienstTag geplanterDienstTag)
        {
            Eintraege.Remove(geplanterDienstTag);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDienstbesetzungsAusfallRepository : IDienstbesetzungsAusfallRepository
    {
        public Task<GeplanterDienstTagAusfall?> GetAsync(Guid dienstplanPeriodeId, DateOnly dienstDatum, DienstbesetzungsSlotCode besetzungsSlotCode, CancellationToken cancellationToken = default)
            => Task.FromResult<GeplanterDienstTagAusfall?>(null);

        public Task<IReadOnlyList<GeplanterDienstTagAusfall>> GetAlleFuerTagAsync(Guid dienstplanPeriodeId, DateOnly dienstDatum, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<GeplanterDienstTagAusfall>>(Array.Empty<GeplanterDienstTagAusfall>());

        public Task<IReadOnlyList<GeplanterDienstTagAusfall>> GetAlleFuerPeriodeAsync(Guid dienstplanPeriodeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<GeplanterDienstTagAusfall>>(Array.Empty<GeplanterDienstTagAusfall>());

        public Task<IReadOnlyList<DateOnly>> GetUrlaubstageFuerBenutzerUndJahrAsync(string userId, int jahr, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DateOnly>>(Array.Empty<DateOnly>());

        public Task AddAsync(GeplanterDienstTagAusfall ausfall, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(GeplanterDienstTagAusfall ausfall)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDienstplanUrlaubszeitraumRepository : IDienstplanUrlaubszeitraumRepository
    {
        public Task<IReadOnlyList<string>> GetAktiveUserIdsFuerDatumAsync(DateOnly datum, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

    private sealed class FakeAutoplanLernereignisRepository : IAutoplanLernereignisRepository
    {
        public List<AutoplanLernereignis> AddedLernereignisse { get; } = new();

        public int SaveChangesCount { get; private set; }

        public Task AddAsync(AutoplanLernereignis lernereignis, CancellationToken cancellationToken = default)
        {
            AddedLernereignisse.Add(lernereignis);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AutoplanLernereignis>> GetVertretungsLernereignisseAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AutoplanLernereignis>>(Array.Empty<AutoplanLernereignis>());

        public Task<IReadOnlyList<AutoplanLernereignis>> GetGrundbesetzungsLernereignisseAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AutoplanLernereignis>>(Array.Empty<AutoplanLernereignis>());
    }
}