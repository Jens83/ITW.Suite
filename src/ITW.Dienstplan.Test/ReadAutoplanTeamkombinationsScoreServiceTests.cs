using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using Xunit;

namespace ITW.Dienstplan.Test;

public sealed class ReadAutoplanTeamkombinationsScoreServiceTests
{
    [Fact]
    public async Task ExecuteAsync_GibtHoeherenScoreFuerPassendereTeamkombinationZurueck()
    {
        // Arrange
        var repository = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeGrundbesetzungsEreignis("arzt-a", "nfs-1", "nfs-2", new DateOnly(2026, 2, 1), new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsEreignis("arzt-a", "nfs-1", "nfs-2", new DateOnly(2026, 2, 5), new DateTimeOffset(2026, 2, 5, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsEreignis("arzt-a", "nfs-1", "nfs-3", new DateOnly(2026, 2, 10), new DateTimeOffset(2026, 2, 10, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsEreignis("arzt-b", "nfs-1", "nfs-2", new DateOnly(2026, 2, 12), new DateTimeOffset(2026, 2, 12, 8, 0, 0, TimeSpan.Zero))
            ]
        };

        var service = new ReadAutoplanTeamkombinationsScoreService(repository);

        // Act
        var result = await service.ExecuteAsync(
            new ReadAutoplanTeamkombinationsScoreQuery(
                BesetzungsSlotCode: DienstbesetzungsSlotCode.Arzt,
                KandidatenUserIds: ["arzt-a", "arzt-b", "arzt-c"],
                KontextNotfallsanitaeter1UserId: "nfs-1",
                KontextNotfallsanitaeter2UserId: "nfs-2",
                Mindestanzahl: 1));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(3, result.Eintraege.Count);

        var arztA = result.Eintraege.Single(x => x.KandidatUserId == "arzt-a");
        Assert.True(arztA.HatTeamPraeferenz);
        Assert.False(arztA.HatNegativesKorrekturmuster);
        Assert.Equal(2, arztA.AnzahlPassenderKombinationen);
        Assert.Equal(0, arztA.AnzahlPassenderKorrekturen);
        Assert.Equal(3, arztA.GesamtanzahlKandidatImSlot);
        Assert.Equal(0.6667m, arztA.Anteil);
        Assert.Equal(8.33m, arztA.LernBonus);

        var arztB = result.Eintraege.Single(x => x.KandidatUserId == "arzt-b");
        Assert.True(arztB.HatTeamPraeferenz);
        Assert.False(arztB.HatNegativesKorrekturmuster);
        Assert.Equal(1, arztB.AnzahlPassenderKombinationen);
        Assert.Equal(0, arztB.AnzahlPassenderKorrekturen);
        Assert.Equal(1, arztB.GesamtanzahlKandidatImSlot);
        Assert.Equal(1.0000m, arztB.Anteil);
        Assert.Equal(10.00m, arztB.LernBonus);

        var arztC = result.Eintraege.Single(x => x.KandidatUserId == "arzt-c");
        Assert.False(arztC.HatTeamPraeferenz);
        Assert.False(arztC.HatNegativesKorrekturmuster);
        Assert.Equal(0, arztC.AnzahlPassenderKombinationen);
        Assert.Equal(0, arztC.AnzahlPassenderKorrekturen);
        Assert.Equal(0m, arztC.LernBonus);
    }

    [Fact]
    public async Task ExecuteAsync_BeruecksichtigtNegativeKorrekturenAlsMalusImGleichenKontext()
    {
        // Arrange
        var repository = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeGrundbesetzungsEreignis("arzt-a", "nfs-1", "nfs-2", new DateOnly(2026, 2, 1), new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsEreignis("arzt-a", "nfs-1", "nfs-2", new DateOnly(2026, 2, 5), new DateTimeOffset(2026, 2, 5, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsEreignis("arzt-b", "nfs-1", "nfs-2", new DateOnly(2026, 2, 7), new DateTimeOffset(2026, 2, 7, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsEreignis("arzt-b", "nfs-1", "nfs-2", new DateOnly(2026, 2, 9), new DateTimeOffset(2026, 2, 9, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsKorrekturEreignis("arzt-a", "arzt-c", "nfs-1", "nfs-2", new DateOnly(2026, 2, 10), new DateTimeOffset(2026, 2, 10, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsKorrekturEreignis("arzt-a", "arzt-c", "nfs-1", "nfs-2", new DateOnly(2026, 2, 12), new DateTimeOffset(2026, 2, 12, 8, 0, 0, TimeSpan.Zero))
            ]
        };

        var service = new ReadAutoplanTeamkombinationsScoreService(repository);

        // Act
        var result = await service.ExecuteAsync(
            new ReadAutoplanTeamkombinationsScoreQuery(
                BesetzungsSlotCode: DienstbesetzungsSlotCode.Arzt,
                KandidatenUserIds: ["arzt-a", "arzt-b"],
                KontextNotfallsanitaeter1UserId: "nfs-1",
                KontextNotfallsanitaeter2UserId: "nfs-2",
                Mindestanzahl: 2,
                MindestanzahlNegativeKorrekturen: 2));

        // Assert
        Assert.True(result.IsSuccess);

        var arztA = result.Eintraege.Single(x => x.KandidatUserId == "arzt-a");
        var arztB = result.Eintraege.Single(x => x.KandidatUserId == "arzt-b");

        Assert.True(arztA.HatTeamPraeferenz);
        Assert.True(arztA.HatNegativesKorrekturmuster);
        Assert.Equal(2, arztA.AnzahlPassenderKombinationen);
        Assert.Equal(2, arztA.AnzahlPassenderKorrekturen);
        Assert.Equal(0m, arztA.LernBonus);

        Assert.True(arztB.HatTeamPraeferenz);
        Assert.False(arztB.HatNegativesKorrekturmuster);
        Assert.True(arztB.LernBonus > 0m);
        Assert.True(arztB.LernBonus > arztA.LernBonus);
    }

    [Fact]
    public async Task ExecuteAsync_GewichtetNeuereLernereignisseStaerkerAlsAlte()
    {
        // Arrange
        var bewertungsDatum = new DateOnly(2026, 4, 1);

        var repository = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeGrundbesetzungsEreignis("arzt-alt", "nfs-1", "nfs-2", new DateOnly(2025, 9, 1), new DateTimeOffset(2025, 9, 1, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsEreignis("arzt-alt", "nfs-1", "nfs-2", new DateOnly(2025, 9, 5), new DateTimeOffset(2025, 9, 5, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsEreignis("arzt-neu", "nfs-1", "nfs-2", new DateOnly(2026, 3, 20), new DateTimeOffset(2026, 3, 20, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeGrundbesetzungsEreignis("arzt-neu", "nfs-1", "nfs-2", new DateOnly(2026, 3, 25), new DateTimeOffset(2026, 3, 25, 8, 0, 0, TimeSpan.Zero))
            ]
        };

        var service = new ReadAutoplanTeamkombinationsScoreService(repository);

        // Act
        var result = await service.ExecuteAsync(
            new ReadAutoplanTeamkombinationsScoreQuery(
                BesetzungsSlotCode: DienstbesetzungsSlotCode.Arzt,
                KandidatenUserIds: ["arzt-alt", "arzt-neu"],
                KontextNotfallsanitaeter1UserId: "nfs-1",
                KontextNotfallsanitaeter2UserId: "nfs-2",
                BewertungsDatum: bewertungsDatum,
                Mindestanzahl: 2));

        // Assert
        Assert.True(result.IsSuccess);

        var alterArzt = result.Eintraege.Single(x => x.KandidatUserId == "arzt-alt");
        var neuerArzt = result.Eintraege.Single(x => x.KandidatUserId == "arzt-neu");

        Assert.True(alterArzt.HatTeamPraeferenz);
        Assert.True(neuerArzt.HatTeamPraeferenz);
        Assert.Equal(0.50m, alterArzt.GewichtetePassendeKombinationen);
        Assert.Equal(2.00m, neuerArzt.GewichtetePassendeKombinationen);
        Assert.True(neuerArzt.LernBonus > alterArzt.LernBonus);
    }

    [Fact]
    public async Task ExecuteAsync_GibtOhneKontextFuerAlleKandidatenNullwerteZurueck()
    {
        // Arrange
        var repository = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeGrundbesetzungsEreignis("arzt-a", "nfs-1", "nfs-2", new DateOnly(2026, 2, 1), new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero))
            ]
        };

        var service = new ReadAutoplanTeamkombinationsScoreService(repository);

        // Act
        var result = await service.ExecuteAsync(
            new ReadAutoplanTeamkombinationsScoreQuery(
                BesetzungsSlotCode: DienstbesetzungsSlotCode.Arzt,
                KandidatenUserIds: ["arzt-a", "arzt-b"],
                Mindestanzahl: 1));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Eintraege.Count);
        Assert.All(result.Eintraege, x =>
        {
            Assert.False(x.HatTeamPraeferenz);
            Assert.False(x.HatNegativesKorrekturmuster);
            Assert.Equal(0, x.AnzahlPassenderKombinationen);
            Assert.Equal(0, x.AnzahlPassenderKorrekturen);
            Assert.Equal(0m, x.LernBonus);
        });
    }

    [Fact]
    public async Task ExecuteAsync_BeruecksichtigtBestaetigteAutoplanVorschlaegeAlsSchwaecherenPositivenLernimpuls()
    {
        // Arrange
        var repository = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeGrundbesetzungsEreignis("arzt-manual", "nfs-1", "nfs-2", new DateOnly(2026, 2, 1), new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero)),
            ErzeugeGrundbesetzungsEreignis("arzt-manual", "nfs-1", "nfs-2", new DateOnly(2026, 2, 5), new DateTimeOffset(2026, 2, 5, 8, 0, 0, TimeSpan.Zero)),
            ErzeugeAutoplanBestaetigungsEreignis("arzt-auto", "nfs-1", "nfs-2", new DateOnly(2026, 2, 10), new DateTimeOffset(2026, 2, 10, 8, 0, 0, TimeSpan.Zero)),
            ErzeugeAutoplanBestaetigungsEreignis("arzt-auto", "nfs-1", "nfs-2", new DateOnly(2026, 2, 12), new DateTimeOffset(2026, 2, 12, 8, 0, 0, TimeSpan.Zero))
            ]
        };

        var service = new ReadAutoplanTeamkombinationsScoreService(repository);

        // Act
        var result = await service.ExecuteAsync(
            new ReadAutoplanTeamkombinationsScoreQuery(
                BesetzungsSlotCode: DienstbesetzungsSlotCode.Arzt,
                KandidatenUserIds: ["arzt-manual", "arzt-auto"],
                KontextNotfallsanitaeter1UserId: "nfs-1",
                KontextNotfallsanitaeter2UserId: "nfs-2",
                Mindestanzahl: 2));

        // Assert
        Assert.True(result.IsSuccess);

        var manuellerKandidat = result.Eintraege.Single(x => x.KandidatUserId == "arzt-manual");
        var autoplanKandidat = result.Eintraege.Single(x => x.KandidatUserId == "arzt-auto");

        Assert.True(manuellerKandidat.HatTeamPraeferenz);
        Assert.True(autoplanKandidat.HatTeamPraeferenz);

        Assert.True(manuellerKandidat.LernBonus > 0m);
        Assert.True(autoplanKandidat.LernBonus > 0m);
        Assert.True(manuellerKandidat.LernBonus > autoplanKandidat.LernBonus);
    }

    private static AutoplanLernereignis ErzeugeGrundbesetzungsEreignis(
    string arztUserId,
    string nfs1UserId,
    string nfs2UserId,
    DateOnly datum,
    DateTimeOffset erfasstAm)
    {
        var vorherigeUserId = string.Equals(arztUserId, "arzt-alt", StringComparison.OrdinalIgnoreCase)
            ? "arzt-vorher"
            : "arzt-alt";

        return new AutoplanLernereignis(
            Guid.NewGuid(),
            Guid.NewGuid(),
            datum,
            DienstbesetzungsSlotCode.Arzt,
            AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert,
            vorherigeUserId: vorherigeUserId,
            neueUserId: arztUserId,
            urspruenglichGeplanterUserId: null,
            kontextArztUserId: arztUserId,
            kontextNotfallsanitaeter1UserId: nfs1UserId,
            kontextNotfallsanitaeter2UserId: nfs2UserId,
            ausfallGrundCode: null,
            bearbeitetVonUserId: "wachleiter-1",
            erfasstAm: erfasstAm);
    }

    private static AutoplanLernereignis ErzeugeAutoplanBestaetigungsEreignis(
    string arztUserId,
    string nfs1UserId,
    string nfs2UserId,
    DateOnly datum,
    DateTimeOffset erfasstAm)
    {
        return new AutoplanLernereignis(
            Guid.NewGuid(),
            Guid.NewGuid(),
            datum,
            DienstbesetzungsSlotCode.Arzt,
            AutoplanLernereignisTypCode.AutoplanVorschlagBestaetigt,
            vorherigeUserId: null,
            neueUserId: arztUserId,
            urspruenglichGeplanterUserId: null,
            kontextArztUserId: arztUserId,
            kontextNotfallsanitaeter1UserId: nfs1UserId,
            kontextNotfallsanitaeter2UserId: nfs2UserId,
            ausfallGrundCode: null,
            bearbeitetVonUserId: "wachleiter-1",
            erfasstAm: erfasstAm);
    }

    private static AutoplanLernereignis ErzeugeGrundbesetzungsKorrekturEreignis(
        string vorherigeUserId,
        string neueUserId,
        string nfs1UserId,
        string nfs2UserId,
        DateOnly datum,
        DateTimeOffset erfasstAm)
    {
        return new AutoplanLernereignis(
            Guid.NewGuid(),
            Guid.NewGuid(),
            datum,
            DienstbesetzungsSlotCode.Arzt,
            AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert,
            vorherigeUserId: vorherigeUserId,
            neueUserId: neueUserId,
            urspruenglichGeplanterUserId: null,
            kontextArztUserId: neueUserId,
            kontextNotfallsanitaeter1UserId: nfs1UserId,
            kontextNotfallsanitaeter2UserId: nfs2UserId,
            ausfallGrundCode: null,
            bearbeitetVonUserId: "wachleiter-1",
            erfasstAm: erfasstAm);
    }

    private sealed class FakeAutoplanLernereignisRepository : IAutoplanLernereignisRepository
    {
        public IReadOnlyList<AutoplanLernereignis> Lernereignisse { get; set; }
            = Array.Empty<AutoplanLernereignis>();

        public Task AddAsync(
            AutoplanLernereignis lernereignis,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<AutoplanLernereignis>> GetVertretungsLernereignisseAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AutoplanLernereignis>>(Array.Empty<AutoplanLernereignis>());

        public Task<IReadOnlyList<AutoplanLernereignis>> GetGrundbesetzungsLernereignisseAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(Lernereignisse);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}