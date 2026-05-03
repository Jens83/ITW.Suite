using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using Xunit;

namespace ITW.Dienstplan.Test;

public sealed class ReadAutoplanVertreterPraeferenzScoreServiceTests
{
    [Fact]
    public async Task ExecuteAsync_GibtFuerPassendenVertreterEinenHoeherenLernbonusZurueck()
    {
        // Arrange
        var repository = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 1), new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 5), new DateTimeOffset(2026, 2, 5, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-c", new DateOnly(2026, 2, 10), new DateTimeOffset(2026, 2, 10, 8, 0, 0, TimeSpan.Zero))
            ]
        };

        var praeferenzenService = new ReadAutoplanVertreterPraeferenzenService(repository);
        var scoreService = new ReadAutoplanVertreterPraeferenzScoreService(praeferenzenService);

        // Act
        var result = await scoreService.ExecuteAsync(
            new ReadAutoplanVertreterPraeferenzScoreQuery(
                UrspruenglichGeplanterUserId: "arzt-a",
                BesetzungsSlotCode: DienstbesetzungsSlotCode.Arzt,
                KandidatenUserIds: ["arzt-b", "arzt-c", "arzt-d"],
                Mindestanzahl: 1));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(3, result.Eintraege.Count);

        var ersterEintrag = result.Eintraege[0];
        Assert.Equal("arzt-b", ersterEintrag.KandidatUserId);
        Assert.True(ersterEintrag.HatPraeferenz);
        Assert.False(ersterEintrag.HatAusfallgrundPraeferenz);
        Assert.Equal(2, ersterEintrag.Anzahl);
        Assert.Equal(3, ersterEintrag.GesamtanzahlFuerUrspruenglichGeplanten);
        Assert.Equal(0.6667m, ersterEintrag.Anteil);
        Assert.Equal(2m, ersterEintrag.GewichteteAnzahl);
        Assert.Equal(7.67m, ersterEintrag.LernBonus);

        var zweiterEintrag = result.Eintraege.Single(x => x.KandidatUserId == "arzt-c");
        Assert.True(zweiterEintrag.HatPraeferenz);
        Assert.False(zweiterEintrag.HatAusfallgrundPraeferenz);
        Assert.Equal(1, zweiterEintrag.Anzahl);
        Assert.Equal(3, zweiterEintrag.GesamtanzahlFuerUrspruenglichGeplanten);
        Assert.Equal(0.3333m, zweiterEintrag.Anteil);
        Assert.Equal(1m, zweiterEintrag.GewichteteAnzahl);
        Assert.Equal(3.33m, zweiterEintrag.LernBonus);

        var dritterEintrag = result.Eintraege.Single(x => x.KandidatUserId == "arzt-d");
        Assert.False(dritterEintrag.HatPraeferenz);
        Assert.False(dritterEintrag.HatAusfallgrundPraeferenz);
        Assert.Equal(0m, dritterEintrag.LernBonus);
    }

    [Fact]
    public async Task ExecuteAsync_BevorzugtAusfallgrundSpezifischePraeferenzVorDemAllgemeinenFallback()
    {
        // Arrange
        var repository = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 1), new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Krankheit),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 5), new DateTimeOffset(2026, 2, 5, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Krankheit),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-c", new DateOnly(2026, 2, 10), new DateTimeOffset(2026, 2, 10, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Urlaub),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-c", new DateOnly(2026, 2, 12), new DateTimeOffset(2026, 2, 12, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Urlaub)
            ]
        };

        var praeferenzenService = new ReadAutoplanVertreterPraeferenzenService(repository);
        var scoreService = new ReadAutoplanVertreterPraeferenzScoreService(praeferenzenService);

        // Act
        var result = await scoreService.ExecuteAsync(
            new ReadAutoplanVertreterPraeferenzScoreQuery(
                UrspruenglichGeplanterUserId: "arzt-a",
                BesetzungsSlotCode: DienstbesetzungsSlotCode.Arzt,
                KandidatenUserIds: ["arzt-b", "arzt-c"],
                Mindestanzahl: 1,
                AusfallGrundCode: DienstausfallGrundCode.Urlaub));

        // Assert
        Assert.True(result.IsSuccess);

        var kandidatB = result.Eintraege.Single(x => x.KandidatUserId == "arzt-b");
        var kandidatC = result.Eintraege.Single(x => x.KandidatUserId == "arzt-c");

        Assert.False(kandidatB.HatPraeferenz);
        Assert.False(kandidatB.HatAusfallgrundPraeferenz);
        Assert.Equal(0m, kandidatB.LernBonus);

        Assert.True(kandidatC.HatPraeferenz);
        Assert.True(kandidatC.HatAusfallgrundPraeferenz);
        Assert.Equal(DienstausfallGrundCode.Urlaub, kandidatC.AusfallGrundCode);
        Assert.True(kandidatC.LernBonus > 0m);
        Assert.True(kandidatC.LernBonus > kandidatB.LernBonus);
    }

    [Fact]
    public async Task ExecuteAsync_FaelltAufAllgemeinePraeferenzZurueck_WennKeinPassenderAusfallgrundVorliegt()
    {
        // Arrange
        var repository = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 1), new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Krankheit),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 5), new DateTimeOffset(2026, 2, 5, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Krankheit)
            ]
        };

        var praeferenzenService = new ReadAutoplanVertreterPraeferenzenService(repository);
        var scoreService = new ReadAutoplanVertreterPraeferenzScoreService(praeferenzenService);

        // Act
        var result = await scoreService.ExecuteAsync(
            new ReadAutoplanVertreterPraeferenzScoreQuery(
                UrspruenglichGeplanterUserId: "arzt-a",
                BesetzungsSlotCode: DienstbesetzungsSlotCode.Arzt,
                KandidatenUserIds: ["arzt-b"],
                Mindestanzahl: 1,
                AusfallGrundCode: DienstausfallGrundCode.Urlaub));

        // Assert
        Assert.True(result.IsSuccess);

        var kandidatB = Assert.Single(result.Eintraege);

        Assert.True(kandidatB.HatPraeferenz);
        Assert.False(kandidatB.HatAusfallgrundPraeferenz);
        Assert.Null(kandidatB.AusfallGrundCode);
        Assert.True(kandidatB.LernBonus > 0m);
    }

    private static AutoplanLernereignis ErzeugeVertretungsEreignis(
        string urspruenglichGeplanterUserId,
        string neueUserId,
        DateOnly datum,
        DateTimeOffset erfasstAm,
        DienstausfallGrundCode ausfallGrundCode = DienstausfallGrundCode.Krankheit,
        DienstbesetzungsSlotCode slotCode = DienstbesetzungsSlotCode.Arzt)
    {
        return new AutoplanLernereignis(
            Guid.NewGuid(),
            Guid.NewGuid(),
            datum,
            slotCode,
            AutoplanLernereignisTypCode.VertretungManuellGeaendert,
            vorherigeUserId: urspruenglichGeplanterUserId,
            neueUserId: neueUserId,
            urspruenglichGeplanterUserId: urspruenglichGeplanterUserId,
            kontextArztUserId: null,
            kontextNotfallsanitaeter1UserId: null,
            kontextNotfallsanitaeter2UserId: null,
            ausfallGrundCode: ausfallGrundCode,
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
            => Task.FromResult(Lernereignisse);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<AutoplanLernereignis>> GetGrundbesetzungsLernereignisseAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AutoplanLernereignis>>(Array.Empty<AutoplanLernereignis>());
    }
}