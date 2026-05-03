using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using Xunit;

namespace ITW.Dienstplan.Test;

public sealed class ReadAutoplanVertreterPraeferenzenServiceTests
{
    [Fact]
    public async Task ExecuteAsync_GruppiertBevorzugteVertreterKorrekt()
    {
        // Arrange
        var repo = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 1), new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 5), new DateTimeOffset(2026, 2, 5, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-c", new DateOnly(2026, 2, 10), new DateTimeOffset(2026, 2, 10, 8, 0, 0, TimeSpan.Zero)),
                ErzeugeVertretungsEreignis("nfs-a", "nfs-b", new DateOnly(2026, 2, 3), new DateTimeOffset(2026, 2, 3, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Krankheit, DienstbesetzungsSlotCode.Notfallsanitaeter1)
            ]
        };

        var service = new ReadAutoplanVertreterPraeferenzenService(repo);

        // Act
        var result = await service.ExecuteAsync(new ReadAutoplanVertreterPraeferenzenQuery(Mindestanzahl: 2));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var eintrag = Assert.Single(result.Eintraege);

        Assert.Equal("arzt-a", eintrag.UrspruenglichGeplanterUserId);
        Assert.Equal(DienstbesetzungsSlotCode.Arzt, eintrag.BesetzungsSlotCode);
        Assert.Equal("arzt-b", eintrag.VertretungsUserId);
        Assert.Equal(2, eintrag.Anzahl);
        Assert.Equal(3, eintrag.GesamtanzahlFuerUrspruenglichGeplanten);
        Assert.Equal(2m, eintrag.GewichteteAnzahl);
        Assert.Equal(3m, eintrag.GewichteteGesamtanzahlFuerUrspruenglichGeplanten);
        Assert.Equal(0.6667m, eintrag.Anteil);
        Assert.Equal(new DateTimeOffset(2026, 2, 5, 8, 0, 0, TimeSpan.Zero), eintrag.LetzteVerwendungAm);
    }

    [Fact]
    public async Task ExecuteAsync_FiltertBeiAusfallgrundNurPassendeVertretungen()
    {
        // Arrange
        var repo = new FakeAutoplanLernereignisRepository
        {
            Lernereignisse =
            [
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 1), new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Krankheit),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 5), new DateTimeOffset(2026, 2, 5, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Krankheit),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-c", new DateOnly(2026, 2, 10), new DateTimeOffset(2026, 2, 10, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Urlaub),
                ErzeugeVertretungsEreignis("arzt-a", "arzt-c", new DateOnly(2026, 2, 12), new DateTimeOffset(2026, 2, 12, 8, 0, 0, TimeSpan.Zero), DienstausfallGrundCode.Urlaub)
            ]
        };

        var service = new ReadAutoplanVertreterPraeferenzenService(repo);

        // Act
        var result = await service.ExecuteAsync(
            new ReadAutoplanVertreterPraeferenzenQuery(
                Mindestanzahl: 1,
                AusfallGrundCode: DienstausfallGrundCode.Urlaub));

        // Assert
        Assert.True(result.IsSuccess);
        var eintrag = Assert.Single(result.Eintraege);

        Assert.Equal("arzt-c", eintrag.VertretungsUserId);
        Assert.Equal(DienstausfallGrundCode.Urlaub, eintrag.AusfallGrundCode);
        Assert.Equal(2, eintrag.Anzahl);
        Assert.Equal(2, eintrag.GesamtanzahlFuerUrspruenglichGeplanten);
        Assert.Equal(1.0000m, eintrag.Anteil);
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