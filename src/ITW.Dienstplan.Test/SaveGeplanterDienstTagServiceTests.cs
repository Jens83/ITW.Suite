using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using Xunit;

namespace ITW.Dienstplan.Test;

public sealed class SaveGeplanterDienstTagServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ErlaubtAenderungUnbeteiligterSlots_TrotzBestehendemAusfallInAnderemSlot()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var datum = new DateOnly(2026, 2, 3);

        var periodeRepository = new FakeDienstplanPeriodeRepository
        {
            Periode = ErzeugePeriode(periodeId, 2026, 2)
        };

        var geplanterDienstTag = new GeplanterDienstTag(
            Guid.NewGuid(),
            periodeId,
            datum,
            arztUserId: "arzt.vertretung",
            notfallsanitaeter1UserId: "nfs.alt.1",
            notfallsanitaeter2UserId: "nfs.alt.2",
            aktualisiertVonUserId: "wachleiter-1",
            aktualisiertAm: DateTimeOffset.UtcNow);

        var geplanterDienstTagRepository = new FakeGeplanterDienstTagRepository
        {
            Eintrag = geplanterDienstTag
        };

        var ausfallRepository = new FakeDienstbesetzungsAusfallRepository
        {
            Ausfaelle =
            [
                new GeplanterDienstTagAusfall(
                    Guid.NewGuid(),
                    periodeId,
                    datum,
                    DienstbesetzungsSlotCode.Arzt,
                    "arzt.original",
                    DienstausfallGrundCode.Krankheit,
                    "arzt.vertretung",
                    "wachleiter-1",
                    DateTimeOffset.UtcNow)
            ]
        };

        var autoplanLernereignisRepository = new FakeAutoplanLernereignisRepository();

        var service = new SaveGeplanterDienstTagService(
            periodeRepository,
            geplanterDienstTagRepository,
            ausfallRepository,
            autoplanLernereignisRepository);

        // Act
        var result = await service.ExecuteAsync(
            new SaveGeplanterDienstTagCommand(
                periodeId,
                datum,
                ArztUserId: "arzt.vertretung",
                Notfallsanitaeter1UserId: "nfs.neu.1",
                Notfallsanitaeter2UserId: "nfs.alt.2",
                BearbeitetVonUserId: "wachleiter-1"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        Assert.Equal("arzt.vertretung", geplanterDienstTag.ArztUserId);
        Assert.Equal("nfs.neu.1", geplanterDienstTag.Notfallsanitaeter1UserId);
        Assert.Equal("nfs.alt.2", geplanterDienstTag.Notfallsanitaeter2UserId);
    }

    [Fact]
    public async Task ExecuteAsync_BlockiertAenderungDesBetroffenenSlots_WennDortBereitsEinAusfallVorliegt()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var datum = new DateOnly(2026, 2, 3);

        var periodeRepository = new FakeDienstplanPeriodeRepository
        {
            Periode = ErzeugePeriode(periodeId, 2026, 2)
        };

        var geplanterDienstTag = new GeplanterDienstTag(
            Guid.NewGuid(),
            periodeId,
            datum,
            arztUserId: "arzt.vertretung",
            notfallsanitaeter1UserId: "nfs.alt.1",
            notfallsanitaeter2UserId: "nfs.alt.2",
            aktualisiertVonUserId: "wachleiter-1",
            aktualisiertAm: DateTimeOffset.UtcNow);

        var geplanterDienstTagRepository = new FakeGeplanterDienstTagRepository
        {
            Eintrag = geplanterDienstTag
        };

        var ausfallRepository = new FakeDienstbesetzungsAusfallRepository
        {
            Ausfaelle =
            [
                new GeplanterDienstTagAusfall(
                    Guid.NewGuid(),
                    periodeId,
                    datum,
                    DienstbesetzungsSlotCode.Arzt,
                    "arzt.original",
                    DienstausfallGrundCode.Krankheit,
                    "arzt.vertretung",
                    "wachleiter-1",
                    DateTimeOffset.UtcNow)
            ]
        };

        var autoplanLernereignisRepository = new FakeAutoplanLernereignisRepository();

        var service = new SaveGeplanterDienstTagService(
            periodeRepository,
            geplanterDienstTagRepository,
            ausfallRepository,
            autoplanLernereignisRepository);

        // Act
        var result = await service.ExecuteAsync(
            new SaveGeplanterDienstTagCommand(
                periodeId,
                datum,
                ArztUserId: "arzt.anders",
                Notfallsanitaeter1UserId: "nfs.neu.1",
                Notfallsanitaeter2UserId: "nfs.alt.2",
                BearbeitetVonUserId: "wachleiter-1"));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Slot Arzt", result.ErrorMessage);
        Assert.Contains("Ausfall & Vertretung", result.ErrorMessage);

        Assert.Equal("arzt.vertretung", geplanterDienstTag.ArztUserId);
        Assert.Equal("nfs.alt.1", geplanterDienstTag.Notfallsanitaeter1UserId);
        Assert.Equal("nfs.alt.2", geplanterDienstTag.Notfallsanitaeter2UserId);
    }

    private static DienstplanPeriode ErzeugePeriode(Guid periodeId, int jahr, int monat)
    {
        return new DienstplanPeriode(
            periodeId,
            jahr,
            monat,
            $"{monat:00}/{jahr}",
            wunschphaseIstOffen: false,
            erstelltVonUserId: "wachleiter-1",
            erstelltAm: DateTimeOffset.UtcNow);
    }

    private sealed class FakeDienstplanPeriodeRepository : IDienstplanPeriodeRepository
    {
        public DienstplanPeriode? Periode { get; set; }

        public Task<bool> ExistiertAsync(int jahr, int monat, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddAsync(DienstplanPeriode periode, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<DienstplanPeriode?> GetByIdAsync(Guid periodeId, CancellationToken cancellationToken = default)
            => Task.FromResult(Periode);

        public Task<DienstplanPeriode?> GetAktuelleOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<DienstplanPeriode?>(null);

        public Task<int> CountOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<IReadOnlyList<DienstplanPeriode>> GetOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DienstplanPeriode>>(Array.Empty<DienstplanPeriode>());

        public Task<IReadOnlyList<DienstplanPeriode>> GetAlleAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DienstplanPeriode>>(Periode is null ? Array.Empty<DienstplanPeriode>() : [Periode]);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeGeplanterDienstTagRepository : IGeplanterDienstTagRepository
    {
        public GeplanterDienstTag? Eintrag { get; set; }

        public Task<GeplanterDienstTag?> GetAsync(Guid dienstplanPeriodeId, DateOnly dienstDatum, CancellationToken cancellationToken = default)
            => Task.FromResult(Eintrag);

        public Task<IReadOnlyList<GeplanterDienstTag>> GetAlleFuerPeriodeAsync(Guid dienstplanPeriodeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<GeplanterDienstTag>>(Eintrag is null ? Array.Empty<GeplanterDienstTag>() : [Eintrag]);

        public Task AddAsync(GeplanterDienstTag geplanterDienstTag, CancellationToken cancellationToken = default)
        {
            Eintrag = geplanterDienstTag;
            return Task.CompletedTask;
        }

        public void Remove(GeplanterDienstTag geplanterDienstTag)
        {
            if (ReferenceEquals(Eintrag, geplanterDienstTag))
            {
                Eintrag = null;
            }
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDienstbesetzungsAusfallRepository : IDienstbesetzungsAusfallRepository
    {
        public IReadOnlyList<GeplanterDienstTagAusfall> Ausfaelle { get; set; }
            = Array.Empty<GeplanterDienstTagAusfall>();

        public Task<GeplanterDienstTagAusfall?> GetAsync(
            Guid dienstplanPeriodeId,
            DateOnly dienstDatum,
            DienstbesetzungsSlotCode besetzungsSlotCode,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Ausfaelle.FirstOrDefault(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                x.DienstDatum == dienstDatum &&
                x.BesetzungsSlotCode == besetzungsSlotCode));
        }

        public Task<IReadOnlyList<GeplanterDienstTagAusfall>> GetAlleFuerTagAsync(
            Guid dienstplanPeriodeId,
            DateOnly dienstDatum,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<GeplanterDienstTagAusfall>>(Ausfaelle
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId && x.DienstDatum == dienstDatum)
                .ToArray());
        }

        public Task<IReadOnlyList<GeplanterDienstTagAusfall>> GetAlleFuerPeriodeAsync(
            Guid dienstplanPeriodeId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<GeplanterDienstTagAusfall>>(Ausfaelle
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
                .ToArray());
        }

        public Task<IReadOnlyList<DateOnly>> GetUrlaubstageFuerBenutzerUndJahrAsync(
            string userId,
            int jahr,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DateOnly>>(Array.Empty<DateOnly>());

        public Task AddAsync(GeplanterDienstTagAusfall ausfall, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(GeplanterDienstTagAusfall ausfall)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeAutoplanLernereignisRepository : IAutoplanLernereignisRepository
    {
        public Task AddAsync(AutoplanLernereignis lernereignis, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<AutoplanLernereignis>> GetVertretungsLernereignisseAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AutoplanLernereignis>>(Array.Empty<AutoplanLernereignis>());

        public Task<IReadOnlyList<AutoplanLernereignis>> GetGrundbesetzungsLernereignisseAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AutoplanLernereignis>>(Array.Empty<AutoplanLernereignis>());

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}