using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using Xunit;

namespace ITW.Dienstplan.Test;

public sealed class ReadAutoplanAllgemeinerVertreterPraeferenzScoreServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ErkenntAllgemeineSlotPraeferenzAbDreiTreffern()
    {
        // Arrange
        var repository = new FakeAutoplanLernereignisRepository
        {
            VertretungsLernereignisse =
            [
                ErzeugeVertretungsEreignis("arzt-a", "arzt-b", new DateOnly(2026, 2, 1)),
                ErzeugeVertretungsEreignis("arzt-c", "arzt-b", new DateOnly(2026, 2, 5)),
                ErzeugeVertretungsEreignis("arzt-d", "arzt-b", new DateOnly(2026, 2, 10)),
                ErzeugeVertretungsEreignis("arzt-e", "arzt-f", new DateOnly(2026, 2, 12))
            ]
        };

        var service = new ReadAutoplanAllgemeinerVertreterPraeferenzScoreService(repository);

        // Act
        var result = await service.ExecuteAsync(
            new ReadAutoplanAllgemeinerVertreterPraeferenzScoreQuery(
                BesetzungsSlotCode: DienstbesetzungsSlotCode.Arzt,
                KandidatenUserIds: ["arzt-b", "arzt-f"],
                Mindestanzahl: 3));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(2, result.Eintraege.Count);

        var arztB = result.Eintraege.Single(x => x.KandidatUserId == "arzt-b");
        Assert.True(arztB.HatPraeferenz);
        Assert.Equal(3, arztB.Anzahl);
        Assert.Equal(4, arztB.GesamtanzahlImSlot);
        Assert.Equal(0.75m, arztB.Anteil);
        Assert.True(arztB.LernBonus > 0m);

        var arztF = result.Eintraege.Single(x => x.KandidatUserId == "arzt-f");
        Assert.False(arztF.HatPraeferenz);
        Assert.Equal(1, arztF.Anzahl);
        Assert.Equal(0m, arztF.LernBonus);
    }

    private static AutoplanLernereignis ErzeugeVertretungsEreignis(
        string urspruenglichGeplanterUserId,
        string neueUserId,
        DateOnly datum)
    {
        return new AutoplanLernereignis(
            Guid.NewGuid(),
            Guid.NewGuid(),
            datum,
            DienstbesetzungsSlotCode.Arzt,
            AutoplanLernereignisTypCode.VertretungManuellGeaendert,
            vorherigeUserId: urspruenglichGeplanterUserId,
            neueUserId: neueUserId,
            urspruenglichGeplanterUserId: urspruenglichGeplanterUserId,
            kontextArztUserId: null,
            kontextNotfallsanitaeter1UserId: null,
            kontextNotfallsanitaeter2UserId: null,
            ausfallGrundCode: DienstausfallGrundCode.Krankheit,
            bearbeitetVonUserId: "wachleiter-1",
            erfasstAm: datum.ToDateTime(TimeOnly.MinValue));
    }

    private sealed class FakeAutoplanLernereignisRepository : IAutoplanLernereignisRepository
    {
        public IReadOnlyList<AutoplanLernereignis> VertretungsLernereignisse { get; set; }
            = Array.Empty<AutoplanLernereignis>();

        public Task AddAsync(
            AutoplanLernereignis lernereignis,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<AutoplanLernereignis>> GetVertretungsLernereignisseAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(VertretungsLernereignisse);

        public Task<IReadOnlyList<AutoplanLernereignis>> GetGrundbesetzungsLernereignisseAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AutoplanLernereignis>>(Array.Empty<AutoplanLernereignis>());

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}