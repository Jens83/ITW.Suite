using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Perioden;
using ITW.Dienstplan.Domain.Entities;
using ITW.Domain.Kalender;
using Xunit;

namespace ITW.Dienstplan.Test;

public sealed class SetPlanfreigabeStatusServiceTests
{
    [Fact]
    public async Task ExecuteAsync_GibtFehlerZurueck_WennPlanbarerTagOhneBesatzungFehlt()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var periode = ErzeugePeriode(periodeId, 2026, 2, wunschphaseIstOffen: false);
        var planbareTage = ErmittlePlanbareTage(2026, 2);
        var fehlenderTag = planbareTage[0];

        var periodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = periode
        };

        var geplanterDienstTagRepository = new FakeGeplanterDienstTagRepository
        {
            Eintraege = ErzeugeVollstaendigBesetzteTage(periodeId, planbareTage.Skip(1))
        };

        var service = new SetPlanfreigabeStatusService(
            periodeRepository,
            geplanterDienstTagRepository);

        // Act
        var result = await service.ExecuteAsync(
            new SetPlanfreigabeStatusCommand(periodeId, true, "wachleiter-1"));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("für alle planbaren Werktage eine Besatzung gespeichert", result.ErrorMessage);
        Assert.Contains(fehlenderTag.ToString("dd.MM.yyyy"), result.ErrorMessage);
        Assert.False(periode.PlanIstFreigegeben);
        Assert.Equal(0, periodeRepository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_GibtFehlerZurueck_WennPlanbarerTagNichtVollstaendigBesetztIst()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var periode = ErzeugePeriode(periodeId, 2026, 2, wunschphaseIstOffen: false);
        var planbareTage = ErmittlePlanbareTage(2026, 2);
        var unterbesetzterTag = planbareTage[1];

        var eintraege = ErzeugeVollstaendigBesetzteTage(periodeId, planbareTage);
        var fehlerhafterEintrag = new GeplanterDienstTag(
            Guid.NewGuid(),
            periodeId,
            unterbesetzterTag,
            arztUserId: "arzt-1",
            notfallsanitaeter1UserId: "nfs-1",
            notfallsanitaeter2UserId: null,
            aktualisiertVonUserId: "wachleiter-1",
            aktualisiertAm: DateTimeOffset.UtcNow);

        eintraege.RemoveAll(x => x.DienstDatum == unterbesetzterTag);
        eintraege.Add(fehlerhafterEintrag);

        var periodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = periode
        };

        var geplanterDienstTagRepository = new FakeGeplanterDienstTagRepository
        {
            Eintraege = eintraege
        };

        var service = new SetPlanfreigabeStatusService(
            periodeRepository,
            geplanterDienstTagRepository);

        // Act
        var result = await service.ExecuteAsync(
            new SetPlanfreigabeStatusCommand(periodeId, true, "wachleiter-1"));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("alle planbaren Werktage vollständig besetzt", result.ErrorMessage);
        Assert.Contains(unterbesetzterTag.ToString("dd.MM.yyyy"), result.ErrorMessage);
        Assert.False(periode.PlanIstFreigegeben);
        Assert.Equal(0, periodeRepository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_IgnoriertWochenendenUndFeiertageBeiDerFreigabepruefung()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var periode = ErzeugePeriode(periodeId, 2026, 5, wunschphaseIstOffen: false);
        var planbareTage = ErmittlePlanbareTage(2026, 5);

        var periodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = periode
        };

        var geplanterDienstTagRepository = new FakeGeplanterDienstTagRepository
        {
            Eintraege = ErzeugeVollstaendigBesetzteTage(periodeId, planbareTage)
        };

        var service = new SetPlanfreigabeStatusService(
            periodeRepository,
            geplanterDienstTagRepository);

        // Act
        var result = await service.ExecuteAsync(
            new SetPlanfreigabeStatusCommand(periodeId, true, "wachleiter-1"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.True(periode.PlanIstFreigegeben);
        Assert.Equal("wachleiter-1", periode.PlanFreigegebenVonUserId);
        Assert.Equal(1, periodeRepository.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_GibtPlanFrei_WennAllePlanbarenTageVollstaendigBesetztSind()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var periode = ErzeugePeriode(periodeId, 2026, 2, wunschphaseIstOffen: false);
        var planbareTage = ErmittlePlanbareTage(2026, 2);

        var periodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = periode
        };

        var geplanterDienstTagRepository = new FakeGeplanterDienstTagRepository
        {
            Eintraege = ErzeugeVollstaendigBesetzteTage(periodeId, planbareTage)
        };

        var service = new SetPlanfreigabeStatusService(
            periodeRepository,
            geplanterDienstTagRepository);

        // Act
        var result = await service.ExecuteAsync(
            new SetPlanfreigabeStatusCommand(periodeId, true, "wachleiter-1"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.True(periode.PlanIstFreigegeben);
        Assert.Equal("wachleiter-1", periode.PlanFreigegebenVonUserId);
        Assert.NotNull(periode.PlanFreigegebenAm);
        Assert.Equal(1, periodeRepository.SaveChangesCount);
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
            $"{monat:00}/{jahr}",
            wunschphaseIstOffen,
            "wachleiter-1",
            DateTimeOffset.UtcNow);
    }

    private static List<GeplanterDienstTag> ErzeugeVollstaendigBesetzteTage(
        Guid periodeId,
        IEnumerable<DateOnly> tage)
    {
        return tage
            .Select(datum => new GeplanterDienstTag(
                Guid.NewGuid(),
                periodeId,
                datum,
                arztUserId: $"arzt-{datum.Day}",
                notfallsanitaeter1UserId: $"nfs1-{datum.Day}",
                notfallsanitaeter2UserId: $"nfs2-{datum.Day}",
                aktualisiertVonUserId: "wachleiter-1",
                aktualisiertAm: DateTimeOffset.UtcNow))
            .ToList();
    }

    private static IReadOnlyList<DateOnly> ErmittlePlanbareTage(int jahr, int monat)
    {
        var feiertage = MecklenburgVorpommernFeiertage.GetFeiertage(jahr);
        var start = new DateOnly(jahr, monat, 1);
        var ende = start.AddMonths(1).AddDays(-1);

        var tage = new List<DateOnly>();

        for (var datum = start; datum <= ende; datum = datum.AddDays(1))
        {
            if (datum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            if (feiertage.ContainsKey(datum))
            {
                continue;
            }

            tage.Add(datum);
        }

        return tage;
    }

    private sealed class FakeDienstplanPeriodeRepository : IDienstplanPeriodeRepository
    {
        public DienstplanPeriode? PeriodeById { get; set; }

        public int SaveChangesCount { get; private set; }

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

        public Task<IReadOnlyList<DienstplanPeriode>> GetOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DienstplanPeriode>>(Array.Empty<DienstplanPeriode>());

        public Task<IReadOnlyList<DienstplanPeriode>> GetAlleAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DienstplanPeriode>>(PeriodeById is null ? Array.Empty<DienstplanPeriode>() : [PeriodeById]);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeGeplanterDienstTagRepository : IGeplanterDienstTagRepository
    {
        public List<GeplanterDienstTag> Eintraege { get; set; } = new();

        public Task<GeplanterDienstTag?> GetAsync(
            Guid dienstplanPeriodeId,
            DateOnly dienstDatum,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Eintraege.FirstOrDefault(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                x.DienstDatum == dienstDatum));
        }

        public Task<IReadOnlyList<GeplanterDienstTag>> GetAlleFuerPeriodeAsync(
            Guid dienstplanPeriodeId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<GeplanterDienstTag>>(Eintraege
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
                .ToArray());
        }

        public Task AddAsync(
            GeplanterDienstTag geplanterDienstTag,
            CancellationToken cancellationToken = default)
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
}